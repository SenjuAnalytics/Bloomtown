using System.Text;
using System.Text.Json;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Leadership;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Accepts village project proposals from qualified players and routes them into voting.
/// </summary>
public sealed class VillageProjectProposalService
{
    private readonly VillageProjectProposalRepository _proposalRepository;
    private readonly ProjectVotingService _votingService;
    private readonly VillageCouncilService _councilService;
    private readonly VillagePositionService _positionService;
    private readonly PlayerEconomyService _economyService;

    public VillageProjectProposalService(
        VillageProjectProposalRepository proposalRepository,
        ProjectVotingService votingService,
        VillageCouncilService councilService,
        VillagePositionService positionService,
        PlayerEconomyService economyService)
    {
        _proposalRepository = proposalRepository;
        _votingService = votingService;
        _councilService = councilService;
        _positionService = positionService;
        _economyService = economyService;
    }

    public async Task<ProjectProposalResponse> HandleAsync(uint playerEntityId, ProjectProposalRequest request)
    {
        return request.Kind switch
        {
            ProjectProposalRequestKind.Propose => await ProposeAsync(playerEntityId, request),
            ProjectProposalRequestKind.ListProposals => await ListProposalsAsync(playerEntityId),
            ProjectProposalRequestKind.Vote => await _votingService.CastVoteAsync(
                playerEntityId,
                request.ProposalId,
                request.ProjectName,
                request.VoteChoice),
            _ => Fail(
                request.Kind,
                ProjectProposalFailureReason.UnknownRequest,
                "Unknown project proposal request."),
        };
    }

    private async Task<ProjectProposalResponse> ProposeAsync(uint playerEntityId, ProjectProposalRequest request)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                ProjectProposalRequestKind.Propose,
                ProjectProposalFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        // Title gate: Builder+ by default; Project Leaders may propose with Helper+.
        var minimumTitle = _positionService.GetMinimumProposerTitle(playerEntityId);
        if (economy.VillageTitle < minimumTitle)
        {
            var requiredTitle = VillageTitleDisplay.GetName(minimumTitle);
            var currentTitle = VillageTitleDisplay.GetName(economy.VillageTitle);
            Log.Information(
                "Player {PlayerId} ({CurrentTitle}) blocked from proposing — requires {RequiredTitle}.",
                playerEntityId,
                currentTitle,
                requiredTitle);

            return Fail(
                ProjectProposalRequestKind.Propose,
                ProjectProposalFailureReason.InsufficientTitle,
                $"Only players with the {requiredTitle} title or higher can propose projects. Your title: {currentTitle}.");
        }

        var projectName = request.ProjectName.Trim();
        if (string.IsNullOrWhiteSpace(projectName) || projectName.Length > PacketSerializer.MaxProjectProposalNameBytes)
        {
            return Fail(
                ProjectProposalRequestKind.Propose,
                ProjectProposalFailureReason.InvalidProjectName,
                $"Project name must be 1–{PacketSerializer.MaxProjectProposalNameBytes} characters.");
        }

        var requirements = BuildRequirements(request);
        if (requirements.Count == 0)
        {
            return Fail(
                ProjectProposalRequestKind.Propose,
                ProjectProposalFailureReason.InvalidRequirements,
                "Specify at least one resource (wood, stone, apple, or tool) with quantity 1–99.");
        }

        var slug = GenerateUniqueSlug(projectName);
        var resourcesJson = SerializeRequirements(requirements);
        var createdAt = DateTime.UtcNow;
        var tier = ProjectTierClassifier.Classify(requirements);

        var proposalId = await _proposalRepository.InsertAsync(new VillageProjectProposalRecord
        {
            ProposedByPlayerId = playerEntityId,
            ProjectName = projectName,
            ProjectSlug = slug,
            RequiredResourcesJson = resourcesJson,
            Status = ProposalStatus.Pending,
            ProjectTier = tier,
            CreatedAtUtc = createdAt,
        });

        Log.Information(
            "Player {PlayerId} ({Title}) submitted {Tier} proposal #{ProposalId}: '{ProjectName}' ({Slug}).",
            playerEntityId,
            VillageTitleDisplay.GetName(economy.VillageTitle),
            tier,
            proposalId,
            projectName,
            slug);

        string responseMessage;
        if (tier == ProjectImportanceTier.Important)
        {
            await _councilService.StartCouncilReviewAsync(proposalId, projectName, slug, playerEntityId);
            responseMessage =
                $"Important proposal #{proposalId} '{projectName}' submitted for Village Council review ({VillageCouncilConfig.ReviewDurationGameMinutes} game minutes). " +
                $"Council votes: council yes {proposalId} | council no {proposalId}. Check status with 'proposals'.";
        }
        else
        {
            await _votingService.StartVotingAsync(proposalId, projectName, slug, playerEntityId);
            responseMessage =
                $"Small proposal #{proposalId} '{projectName}' submitted. Citizen voting is open for {ProjectVotingConfig.DurationGameMinutes} game minutes. " +
                $"Vote: vote yes on {proposalId} | vote no on {proposalId}. " +
                $"Chief may fast-track once per day: chief approve {proposalId}. Check status with 'proposals'.";
        }

        return new ProjectProposalResponse(
            true,
            ProjectProposalRequestKind.Propose,
            ProjectProposalFailureReason.None,
            responseMessage);
    }

    private async Task<ProjectProposalResponse> ListProposalsAsync(uint playerEntityId)
    {
        var proposals = await _proposalRepository.GetAllAsync();
        var voting = proposals.Where(proposal => proposal.Status == ProposalStatus.Voting).ToList();
        var councilVoting = proposals.Where(proposal => proposal.Status == ProposalStatus.CouncilVoting).ToList();

        var playerTitle = VillageTitle.Newcomer;
        var playerVoteWeight = 1;
        var playerPosition = VillagePosition.None;
        if (_economyService.TryGetState(playerEntityId, out var economy))
        {
            playerTitle = economy.VillageTitle;
            playerVoteWeight = VoteWeightCalculator.GetWeight(playerTitle);
            playerPosition = economy.VillagePosition;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Village Project Proposals:");
        builder.AppendLine();
        builder.AppendLine(
            $"Your vote weight: {playerVoteWeight} ({VillageTitleDisplay.GetName(playerTitle)})");
        builder.AppendLine(
            "Weights: Newcomer/Helper 1, Builder 2, Respected Villager 3, Elder Candidate 4");

        if (voting.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("No small proposals in citizen voting.");
        }
        else
        {
            foreach (var proposal in voting)
            {
                var tally = await _votingService.GetWeightedTallyAsync(proposal.Id);
                var remaining = _votingService.GetMinutesRemaining(proposal);

                builder.AppendLine();
                builder.AppendLine(
                    $"[SMALL/VOTING] #{proposal.Id} '{proposal.ProjectName}' by player {proposal.ProposedByPlayerId}");
                builder.AppendLine($"  Needs: {FormatRequirements(proposal.RequiredResourcesJson)}");
                builder.AppendLine($"  Votes: {tally.FormatCountsAndWeights()} | {remaining} game minute(s) left");
                builder.AppendLine($"  Vote: vote yes on {proposal.Id} | vote no on {proposal.Id}");
                builder.AppendLine($"  Chief fast-track: chief approve {proposal.Id} (once per game day)");

                if (playerTitle == VillageTitle.ElderCandidate)
                {
                    var preview = _votingService.GetElderProjectedOutcome(tally);
                    if (!string.IsNullOrWhiteSpace(preview))
                        builder.AppendLine($"  {preview}");
                }
            }
        }

        if (councilVoting.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("No important proposals in Council review.");
        }
        else
        {
            foreach (var proposal in councilVoting)
            {
                var tally = await _councilService.GetCouncilTallyAsync(proposal.Id);
                var remaining = _councilService.GetMinutesRemaining(proposal);

                builder.AppendLine();
                builder.AppendLine(
                    $"[IMPORTANT/COUNCIL] #{proposal.Id} '{proposal.ProjectName}' by player {proposal.ProposedByPlayerId}");
                builder.AppendLine($"  Needs: {FormatRequirements(proposal.RequiredResourcesJson)}");
                builder.AppendLine($"  Council votes: {tally.FormatCountsAndWeights()} | {remaining} game minute(s) left");

                if (VillageCouncilConfig.IsCouncilPosition(playerPosition))
                {
                    builder.AppendLine($"  Vote: council yes {proposal.Id} | council no {proposal.Id}");
                    if (playerPosition == VillagePosition.Chief)
                        builder.AppendLine($"  Chief veto: chief veto {proposal.Id} (once per game day)");
                }
                else
                {
                    builder.AppendLine("  (Council members only may vote on important proposals.)");
                }
            }
        }

        if (proposals.Any(proposal => proposal.Status == ProposalStatus.Approved))
        {
            builder.AppendLine();
            builder.AppendLine("Recently approved:");
            foreach (var proposal in proposals.Where(p => p.Status == ProposalStatus.Approved).Take(5))
            {
                var projectLabel = proposal.CreatedProjectId.HasValue
                    ? $"project id {proposal.CreatedProjectId.Value}"
                    : "active project";
                builder.AppendLine($"  [APPROVED] #{proposal.Id} '{proposal.ProjectName}' ({proposal.ProjectSlug}) → {projectLabel}");
            }
        }

        if (proposals.Any(proposal => proposal.Status == ProposalStatus.Rejected))
        {
            builder.AppendLine();
            builder.AppendLine("Recently rejected:");
            foreach (var proposal in proposals.Where(p => p.Status == ProposalStatus.Rejected).Take(3))
            {
                builder.AppendLine($"  [REJECTED] #{proposal.Id} '{proposal.ProjectName}'");
            }
        }

        Log.Information("Player {PlayerId} viewed village project proposals.", playerEntityId);

        return new ProjectProposalResponse(
            true,
            ProjectProposalRequestKind.ListProposals,
            ProjectProposalFailureReason.None,
            builder.ToString().TrimEnd());
    }

    private static Dictionary<ItemType, int> BuildRequirements(ProjectProposalRequest request)
    {
        var requirements = new Dictionary<ItemType, int>();

        AddRequirement(requirements, ItemType.Wood, request.WoodQuantity);
        AddRequirement(requirements, ItemType.Stone, request.StoneQuantity);
        AddRequirement(requirements, ItemType.Apple, request.AppleQuantity);
        AddRequirement(requirements, ItemType.Tool, request.ToolQuantity);

        return requirements;
    }

    private static void AddRequirement(Dictionary<ItemType, int> requirements, ItemType itemType, byte quantity)
    {
        if (quantity is < 1 or > 99)
            return;

        requirements[itemType] = quantity;
    }

    private static string GenerateUniqueSlug(string projectName)
    {
        var slug = Slugify(projectName);
        if (!CommunityProjectRegistry.TryGetBySlug(slug, out _))
            return slug;

        for (var suffix = 2; suffix < 100; suffix++)
        {
            var candidate = $"{slug}-{suffix}";
            if (!CommunityProjectRegistry.TryGetBySlug(candidate, out _))
                return candidate;
        }

        return $"project-{Guid.NewGuid():N}"[..Math.Min(40, $"project-{Guid.NewGuid():N}".Length)];
    }

    private static string Slugify(string projectName)
    {
        var lower = projectName.Trim().ToLowerInvariant();
        var chars = lower.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);

        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        slug = slug.Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
            slug = "custom-project";

        return slug.Length <= 40 ? slug : slug[..40];
    }

    private static string SerializeRequirements(IReadOnlyDictionary<ItemType, int> requirements)
    {
        var raw = requirements.ToDictionary(pair => (int)pair.Key, pair => pair.Value);
        return JsonSerializer.Serialize(raw);
    }

    private static string FormatRequirements(string requiredResourcesJson)
    {
        var requirements = CommunityProjectDefinitionRepository.DeserializeRequirements(requiredResourcesJson);
        return string.Join(", ", requirements
            .OrderBy(pair => pair.Key)
            .Select(pair => $"{ItemDatabase.GetDisplayName(pair.Key)} {pair.Value}"));
    }

    private static ProjectProposalResponse Fail(
        ProjectProposalRequestKind kind,
        ProjectProposalFailureReason reason,
        string message)
    {
        Log.Information("Project proposal request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new ProjectProposalResponse(false, kind, reason, message);
    }
}