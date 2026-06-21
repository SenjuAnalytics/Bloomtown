using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// Manages Village Council review, Chief direct approval, and Chief veto authority.
/// </summary>
public sealed class VillageCouncilService : ISimulationSystem
{
    private readonly VillageProjectProposalRepository _proposalRepository;
    private readonly CouncilProposalVoteRepository _councilVoteRepository;
    private readonly ChiefAuthorityLogRepository _chiefAuthorityLogRepository;
    private readonly CommunityProjectService _communityProjectService;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerRepository _playerRepository;
    private readonly WorldTimeSystem _worldTime;
    private long _lastCheckedTotalMinutes = -1;

    public event Action<string>? CouncilReviewStarted;
    public event Action<string>? CouncilReviewEnded;
    public event Action<string>? ChiefAuthorityUsed;

    public VillageCouncilService(
        VillageProjectProposalRepository proposalRepository,
        CouncilProposalVoteRepository councilVoteRepository,
        ChiefAuthorityLogRepository chiefAuthorityLogRepository,
        CommunityProjectService communityProjectService,
        PlayerEconomyService economyService,
        PlayerRepository playerRepository,
        WorldTimeSystem worldTime)
    {
        _proposalRepository = proposalRepository;
        _councilVoteRepository = councilVoteRepository;
        _chiefAuthorityLogRepository = chiefAuthorityLogRepository;
        _communityProjectService = communityProjectService;
        _economyService = economyService;
        _playerRepository = playerRepository;
        _worldTime = worldTime;
    }

    public async Task LoadAsync()
    {
        var currentTotal = GetCurrentTotalMinutes();
        _lastCheckedTotalMinutes = currentTotal;
        await FinalizeExpiredCouncilReviewsAsync(currentTotal);
    }

    public void Update(double deltaTimeSeconds)
    {
        var currentTotal = GetCurrentTotalMinutes();
        if (currentTotal == _lastCheckedTotalMinutes)
            return;

        _lastCheckedTotalMinutes = currentTotal;
        FinalizeExpiredCouncilReviewsAsync(currentTotal).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Routes important proposals into Council review instead of citizen voting.
    /// </summary>
    public async Task StartCouncilReviewAsync(int proposalId, string projectName, string slug, uint proposerId)
    {
        var reviewEndTotal = GameTimeMath.AddGameMinutes(
            _worldTime.GameDay,
            _worldTime.GameMinute,
            VillageCouncilConfig.ReviewDurationGameMinutes);

        await _proposalRepository.UpdateVotingStateAsync(proposalId, ProposalStatus.CouncilVoting, reviewEndTotal);

        Log.Information(
            "Village Council review started for important proposal #{ProposalId} '{ProjectName}' ({Slug}) by player {ProposerId}. Ends at game minute index {ReviewEndTotal}.",
            proposalId,
            projectName,
            slug,
            proposerId,
            reviewEndTotal);

        var message =
            $"Council review started on important proposal #{proposalId}: '{projectName}'. " +
            $"Council members vote: council yes {proposalId} | council no {proposalId}. " +
            $"Chief may veto once per game day: chief veto {proposalId}. " +
            $"Closes in {VillageCouncilConfig.ReviewDurationGameMinutes} game minutes.";
        CouncilReviewStarted?.Invoke(message);
    }

    public async Task<VillagePositionResponse> CastCouncilVoteAsync(
        uint playerEntityId,
        int proposalId,
        ProjectVoteChoice voteChoice)
    {
        if (voteChoice is not (ProjectVoteChoice.Yes or ProjectVoteChoice.No))
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.InvalidVote,
                "Vote must be 'yes' or 'no'. Usage: council yes 5");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!VillageCouncilConfig.IsCouncilPosition(economy.VillagePosition))
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.NotCouncilMember,
                "Only Village Council members (Advisor, Deputy Chief, Chief) may vote in Council review.");
        }

        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.UnknownProposal,
                $"Unknown proposal #{proposalId}. Use 'proposals' to see open reviews.");
        }

        if (proposal.Status != ProposalStatus.CouncilVoting)
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.NotInCouncilVoting,
                $"Proposal #{proposal.Id} '{proposal.ProjectName}' is not in Council review (status: {proposal.Status}).");
        }

        var currentTotal = GetCurrentTotalMinutes();
        if (proposal.VotingEndTotalMinutes.HasValue && currentTotal >= proposal.VotingEndTotalMinutes.Value)
        {
            await FinalizeCouncilReviewAsync(proposal);
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.CouncilVotingClosed,
                $"Council review on proposal #{proposal.Id} has ended.");
        }

        if (await _councilVoteRepository.HasVotedAsync(proposal.Id, playerEntityId))
        {
            return Fail(
                VillagePositionRequestKind.CouncilVote,
                VillagePositionFailureReason.AlreadyVotedCouncil,
                $"You have already voted in Council review for proposal #{proposal.Id}.");
        }

        var voteWeight = VoteWeightCalculator.GetWeight(economy.VillageTitle);
        var titleName = VillageTitleDisplay.GetName(economy.VillageTitle);
        var positionName = VillagePositionDisplay.GetName(economy.VillagePosition);

        await _councilVoteRepository.InsertAsync(new CouncilProposalVoteRecord
        {
            ProposalId = proposal.Id,
            PlayerEntityId = playerEntityId,
            Vote = voteChoice,
            VoteWeight = voteWeight,
            VotedAtUtc = DateTime.UtcNow,
        });

        var tally = await _councilVoteRepository.GetWeightedTallyAsync(proposal.Id);
        var voteLabel = voteChoice == ProjectVoteChoice.Yes ? "Yes" : "No";

        Log.Information(
            "Council member {PlayerId} ({Position}, {Title}, weight {VoteWeight}) voted {Vote} on proposal #{ProposalId} '{ProjectName}'. Council tally: {Tally}.",
            playerEntityId,
            positionName,
            titleName,
            voteWeight,
            voteLabel,
            proposal.Id,
            proposal.ProjectName,
            tally.FormatCountsAndWeights());

        var remaining = proposal.VotingEndTotalMinutes.HasValue
            ? GameTimeMath.MinutesRemaining(currentTotal, proposal.VotingEndTotalMinutes.Value)
            : 0;

        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.CouncilVote,
            VillagePositionFailureReason.None,
            $"Council vote recorded: {voteLabel} on proposal #{proposal.Id} '{proposal.ProjectName}' " +
            $"(you are {positionName}, vote weight {voteWeight}). " +
            $"Council tally: {tally.FormatCountsAndWeights()}. {remaining} game minute(s) remaining.");
    }

    /// <summary>
    /// Chief may fast-track one small proposal per game day without waiting for citizen voting.
    /// </summary>
    public async Task<VillagePositionResponse> ChiefDirectApproveAsync(uint playerEntityId, int proposalId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (economy.VillagePosition != VillagePosition.Chief)
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.NotChief,
                "Only the Chief may use direct approval. Usage: chief approve 5");
        }

        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.UnknownProposal,
                $"Unknown proposal #{proposalId}.");
        }

        if (proposal.ProjectTier != ProjectImportanceTier.Small)
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.NotSmallProject,
                $"Proposal #{proposal.Id} is important — it must go through Village Council, not Chief direct approval.");
        }

        if (proposal.Status != ProposalStatus.Voting)
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.NotInCitizenVoting,
                $"Proposal #{proposal.Id} is not open for citizen voting (status: {proposal.Status}).");
        }

        var usedToday = await _chiefAuthorityLogRepository.CountActionsOnGameDayAsync(
            ChiefAuthorityAction.DirectApprove,
            _worldTime.GameDay);

        if (usedToday >= VillageCouncilConfig.ChiefDirectApprovalsPerGameDay)
        {
            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.ChiefAuthorityLimitReached,
                $"Chief direct approval limit reached for game day {_worldTime.GameDay} " +
                $"(max {VillageCouncilConfig.ChiefDirectApprovalsPerGameDay} per day).");
        }

        var activation = await ProposalActivator.TryActivateAsync(
            _communityProjectService,
            _proposalRepository,
            proposal);

        await _chiefAuthorityLogRepository.InsertAsync(new ChiefAuthorityLogRecord
        {
            ChiefPlayerId = playerEntityId,
            ActionType = ChiefAuthorityAction.DirectApprove,
            ProposalId = proposal.Id,
            GameDay = _worldTime.GameDay,
            CreatedAtUtc = DateTime.UtcNow,
        });

        if (!activation.Success)
        {
            Log.Warning(
                "Chief {PlayerId} direct-approved proposal #{ProposalId} but activation failed.",
                playerEntityId,
                proposal.Id);

            return Fail(
                VillagePositionRequestKind.ChiefApprove,
                VillagePositionFailureReason.UnknownProposal,
                $"Chief approval recorded but project activation failed for proposal #{proposal.Id}.");
        }

        Log.Information(
            "Chief {PlayerId} used direct approval on small proposal #{ProposalId} '{ProjectName}' — community project id {ProjectId} (game day {GameDay}, {UsedToday} approval(s) today).",
            playerEntityId,
            proposal.Id,
            proposal.ProjectName,
            activation.CreatedProjectId,
            _worldTime.GameDay,
            usedToday + 1);

        var message =
            $"Chief direct approval — proposal #{proposal.Id} '{proposal.ProjectName}' is now an active community project ({proposal.ProjectSlug}). " +
            $"Chief authority used: {usedToday + 1}/{VillageCouncilConfig.ChiefDirectApprovalsPerGameDay} today.";
        ChiefAuthorityUsed?.Invoke(message);

        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.ChiefApprove,
            VillagePositionFailureReason.None,
            message);
    }

    /// <summary>
    /// Chief may veto one Council review per game day, rejecting the proposal immediately.
    /// </summary>
    public async Task<VillagePositionResponse> ChiefVetoAsync(uint playerEntityId, int proposalId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillagePositionRequestKind.ChiefVeto,
                VillagePositionFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (economy.VillagePosition != VillagePosition.Chief)
        {
            return Fail(
                VillagePositionRequestKind.ChiefVeto,
                VillagePositionFailureReason.NotChief,
                "Only the Chief may veto Council decisions. Usage: chief veto 5");
        }

        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return Fail(
                VillagePositionRequestKind.ChiefVeto,
                VillagePositionFailureReason.UnknownProposal,
                $"Unknown proposal #{proposalId}.");
        }

        if (proposal.Status != ProposalStatus.CouncilVoting)
        {
            return Fail(
                VillagePositionRequestKind.ChiefVeto,
                VillagePositionFailureReason.NotInCouncilVoting,
                $"Proposal #{proposal.Id} is not in Council review (status: {proposal.Status}).");
        }

        var usedToday = await _chiefAuthorityLogRepository.CountActionsOnGameDayAsync(
            ChiefAuthorityAction.Veto,
            _worldTime.GameDay);

        if (usedToday >= VillageCouncilConfig.ChiefVetoesPerGameDay)
        {
            return Fail(
                VillagePositionRequestKind.ChiefVeto,
                VillagePositionFailureReason.ChiefAuthorityLimitReached,
                $"Chief veto limit reached for game day {_worldTime.GameDay} " +
                $"(max {VillageCouncilConfig.ChiefVetoesPerGameDay} per day).");
        }

        await _proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.Rejected, null);
        await _chiefAuthorityLogRepository.InsertAsync(new ChiefAuthorityLogRecord
        {
            ChiefPlayerId = playerEntityId,
            ActionType = ChiefAuthorityAction.Veto,
            ProposalId = proposal.Id,
            GameDay = _worldTime.GameDay,
            CreatedAtUtc = DateTime.UtcNow,
        });

        Log.Information(
            "Chief {PlayerId} vetoed Council proposal #{ProposalId} '{ProjectName}' (game day {GameDay}, {UsedToday} veto(es) today).",
            playerEntityId,
            proposal.Id,
            proposal.ProjectName,
            _worldTime.GameDay,
            usedToday + 1);

        var message =
            $"Chief veto — proposal #{proposal.Id} '{proposal.ProjectName}' REJECTED by Chief authority. " +
            $"Vetoes used today: {usedToday + 1}/{VillageCouncilConfig.ChiefVetoesPerGameDay}.";
        ChiefAuthorityUsed?.Invoke(message);
        CouncilReviewEnded?.Invoke(message);

        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.ChiefVeto,
            VillagePositionFailureReason.None,
            message);
    }

    public async Task<WeightedVoteTally> GetCouncilTallyAsync(int proposalId)
    {
        return await _councilVoteRepository.GetWeightedTallyAsync(proposalId);
    }

    public int GetMinutesRemaining(VillageProjectProposalRecord proposal)
    {
        if (!proposal.VotingEndTotalMinutes.HasValue)
            return 0;

        return GameTimeMath.MinutesRemaining(GetCurrentTotalMinutes(), proposal.VotingEndTotalMinutes.Value);
    }

    /// <summary>
    /// Extended village information visible only to Village Council members.
    /// </summary>
    public async Task<string> BuildCouncilDashboardAsync(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy)
            || !VillageCouncilConfig.IsCouncilPosition(economy.VillagePosition))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Village Council dashboard (Council privilege):");

        var councilCount = await _playerRepository.CountCouncilMembersAsync();
        builder.AppendLine($"  Council seats filled: {councilCount}/{VillageCouncilConfig.CouncilPositions.Length}");

        foreach (var position in VillageCouncilConfig.CouncilPositions)
        {
            var holderId = await _playerRepository.GetHolderPlayerIdAsync(position);
            builder.AppendLine(
                $"  {VillagePositionDisplay.GetName(position)}: {(holderId.HasValue ? $"player {holderId.Value}" : "(vacant)")}");
        }

        var directApprovals = await _chiefAuthorityLogRepository.CountActionsOnGameDayAsync(
            ChiefAuthorityAction.DirectApprove,
            _worldTime.GameDay);
        var vetoes = await _chiefAuthorityLogRepository.CountActionsOnGameDayAsync(
            ChiefAuthorityAction.Veto,
            _worldTime.GameDay);
        builder.AppendLine(
            $"  Chief authority today (day {_worldTime.GameDay}): direct approvals {directApprovals}/{VillageCouncilConfig.ChiefDirectApprovalsPerGameDay}, vetoes {vetoes}/{VillageCouncilConfig.ChiefVetoesPerGameDay}");

        var councilProposals = await _proposalRepository.GetCouncilVotingAsync();
        if (councilProposals.Count == 0)
        {
            builder.AppendLine("  No proposals in Council review.");
        }
        else
        {
            foreach (var proposal in councilProposals)
            {
                var tally = await _councilVoteRepository.GetWeightedTallyAsync(proposal.Id);
                var remaining = GetMinutesRemaining(proposal);
                builder.AppendLine(
                    $"  [COUNCIL] #{proposal.Id} '{proposal.ProjectName}' — {tally.FormatCountsAndWeights()} | {remaining} min left");
                builder.AppendLine($"    Vote: council yes {proposal.Id} | council no {proposal.Id}");
                if (economy.VillagePosition == VillagePosition.Chief)
                    builder.AppendLine($"    Chief: chief veto {proposal.Id}");
            }
        }

        var recentAuthority = await _chiefAuthorityLogRepository.GetRecentAsync(5);
        if (recentAuthority.Count > 0)
        {
            builder.AppendLine("  Recent Chief authority log:");
            foreach (var entry in recentAuthority)
            {
                builder.AppendLine(
                    $"    Day {entry.GameDay}: {entry.ActionType} on proposal #{entry.ProposalId} by Chief player {entry.ChiefPlayerId}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private async Task FinalizeExpiredCouncilReviewsAsync(long currentTotalMinutes)
    {
        var expired = await _proposalRepository.GetExpiredCouncilVotingAsync(currentTotalMinutes);
        foreach (var proposal in expired)
            await FinalizeCouncilReviewAsync(proposal);
    }

    /// <summary>
    /// Weighted Council majority with adaptive minimum based on seated members.
    /// </summary>
    private async Task FinalizeCouncilReviewAsync(VillageProjectProposalRecord proposal)
    {
        if (proposal.Status != ProposalStatus.CouncilVoting)
            return;

        var tally = await _councilVoteRepository.GetWeightedTallyAsync(proposal.Id);
        var seatedCount = await _playerRepository.CountCouncilMembersAsync();
        var minimumVotes = Math.Min(
            VillageCouncilConfig.MinimumCouncilVotes,
            Math.Max(1, seatedCount));

        var approved = tally.WouldApprove(minimumVotes);

        string message;
        if (approved)
        {
            var activation = await ProposalActivator.TryActivateAsync(
                _communityProjectService,
                _proposalRepository,
                proposal);

            if (!activation.Success)
            {
                message =
                    $"Council review ended — proposal #{proposal.Id} '{proposal.ProjectName}' Council vote passed ({tally.FormatCountsAndWeights()}) but activation failed.";
                CouncilReviewEnded?.Invoke(message);
                return;
            }

            Log.Information(
                "Proposal #{ProposalId} '{ProjectName}' APPROVED by Village Council ({Tally}, voters {TotalVoters}, min {MinimumVotes}) — community project id {ProjectId}.",
                proposal.Id,
                proposal.ProjectName,
                tally.FormatCountsAndWeights(),
                tally.TotalVoters,
                minimumVotes,
                activation.CreatedProjectId);

            message =
                $"Council review ended — proposal #{proposal.Id} '{proposal.ProjectName}' APPROVED ({tally.FormatCountsAndWeights()}). " +
                $"Now an active community project ({proposal.ProjectSlug}). Use 'projects' to contribute.";
        }
        else
        {
            await _proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.Rejected, null);

            var reason = tally.TotalVoters < minimumVotes
                ? $"only {tally.TotalVoters} Council voter(s), need at least {minimumVotes}"
                : $"Yes weight {tally.YesWeight} did not exceed No weight {tally.NoWeight}";

            Log.Information(
                "Proposal #{ProposalId} '{ProjectName}' REJECTED by Village Council ({Tally}) — {Reason}.",
                proposal.Id,
                proposal.ProjectName,
                tally.FormatCountsAndWeights(),
                reason);

            message =
                $"Council review ended — proposal #{proposal.Id} '{proposal.ProjectName}' REJECTED ({tally.FormatCountsAndWeights()}). {reason}.";
        }

        CouncilReviewEnded?.Invoke(message);
    }

    private long GetCurrentTotalMinutes()
    {
        return GameTimeMath.ToTotalGameMinutes(_worldTime.GameDay, _worldTime.GameMinute);
    }

    private static VillagePositionResponse Fail(
        VillagePositionRequestKind kind,
        VillagePositionFailureReason reason,
        string message)
    {
        Log.Information("Village Council request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new VillagePositionResponse(false, kind, reason, message);
    }
}