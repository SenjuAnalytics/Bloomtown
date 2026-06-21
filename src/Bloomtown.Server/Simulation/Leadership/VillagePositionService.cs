using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// Manages village leadership positions, elections, privileges, and persistence.
/// </summary>
public sealed class VillagePositionService : ISimulationSystem
{
    private readonly PlayerRepository _playerRepository;
    private readonly PositionElectionRepository _electionRepository;
    private readonly PositionElectionVoteRepository _electionVoteRepository;
    private readonly VillageCouncilService _councilService;
    private readonly PlayerEconomyService _economyService;
    private readonly WorldTimeSystem _worldTime;
    private long _lastCheckedTotalMinutes = -1;

    public event Action<string>? ElectionStarted;
    public event Action<string>? ElectionEnded;

    public VillagePositionService(
        PlayerRepository playerRepository,
        PositionElectionRepository electionRepository,
        PositionElectionVoteRepository electionVoteRepository,
        VillageCouncilService councilService,
        PlayerEconomyService economyService,
        WorldTimeSystem worldTime)
    {
        _playerRepository = playerRepository;
        _electionRepository = electionRepository;
        _electionVoteRepository = electionVoteRepository;
        _councilService = councilService;
        _economyService = economyService;
        _worldTime = worldTime;
    }

    public async Task LoadAsync()
    {
        var currentTotal = GetCurrentTotalMinutes();
        _lastCheckedTotalMinutes = currentTotal;
        await FinalizeExpiredElectionsAsync(currentTotal);
    }

    public void Update(double deltaTimeSeconds)
    {
        var currentTotal = GetCurrentTotalMinutes();
        if (currentTotal == _lastCheckedTotalMinutes)
            return;

        _lastCheckedTotalMinutes = currentTotal;
        FinalizeExpiredElectionsAsync(currentTotal).GetAwaiter().GetResult();
    }

    public bool HasPosition(uint playerEntityId, VillagePosition position)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return false;

        return economy.VillagePosition == position;
    }

    /// <summary>
    /// Project Leaders may propose communal projects with a lower title requirement (Helper+).
    /// </summary>
    public VillageTitle GetMinimumProposerTitle(uint playerEntityId)
    {
        return HasPosition(playerEntityId, VillagePosition.ProjectLeader)
            ? VillageTitle.Helper
            : VillageTitle.Builder;
    }

    public async Task<VillagePositionResponse> HandleAsync(uint playerEntityId, VillagePositionRequest request)
    {
        return request.Kind switch
        {
            VillagePositionRequestKind.List => await ListPositionsAsync(playerEntityId),
            VillagePositionRequestKind.RunFor => await RunForPositionAsync(playerEntityId, request.Position),
            VillagePositionRequestKind.Vote => await CastElectionVoteAsync(playerEntityId, request.Position, request.VoteChoice),
            VillagePositionRequestKind.CouncilVote => await _councilService.CastCouncilVoteAsync(
                playerEntityId,
                request.ProposalId,
                request.VoteChoice),
            VillagePositionRequestKind.ChiefApprove => await _councilService.ChiefDirectApproveAsync(
                playerEntityId,
                request.ProposalId),
            VillagePositionRequestKind.ChiefVeto => await _councilService.ChiefVetoAsync(
                playerEntityId,
                request.ProposalId),
            _ => Fail(
                request.Kind,
                VillagePositionFailureReason.UnknownRequest,
                "Unknown village position request."),
        };
    }

    private async Task<VillagePositionResponse> ListPositionsAsync(uint playerEntityId)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Village Leadership Positions:");

        if (_economyService.TryGetState(playerEntityId, out var viewerEconomy))
        {
            var viewerPositionLabel = viewerEconomy.VillagePosition == VillagePosition.None
                ? "None"
                : VillagePositionDisplay.GetName(viewerEconomy.VillagePosition);
            builder.AppendLine();
            builder.AppendLine($"Your position: {viewerPositionLabel}");
        }

        builder.AppendLine();
        foreach (var position in GetLeadershipPositions())
        {
            var requirement = VillagePositionConfig.GetRequirement(position);
            var holderId = await _playerRepository.GetHolderPlayerIdAsync(position);
            var holderLabel = holderId.HasValue ? $"player {holderId.Value}" : "(vacant)";
            var election = await _electionRepository.GetActiveByPositionAsync(position);

            builder.AppendLine($"{VillagePositionDisplay.GetName(position)} ({VillagePositionDisplay.GetSlug(position)})");
            builder.AppendLine($"  Holder: {holderLabel}");
            builder.AppendLine(
                $"  Requirements: {VillageTitleDisplay.GetName(requirement.MinimumTitle)}" +
                (requirement.MinimumContributionScore > 0
                    ? $" + {requirement.MinimumContributionScore} contribution"
                    : string.Empty));

            if (election is not null)
            {
                var tally = await _electionVoteRepository.GetWeightedTallyAsync(election.Id);
                var remaining = GameTimeMath.MinutesRemaining(
                    GetCurrentTotalMinutes(),
                    election.VotingEndTotalMinutes);
                builder.AppendLine(
                    $"  Election: candidate player {election.CandidatePlayerId} | {tally.FormatCountsAndWeights()} | {remaining} game min left");
                builder.AppendLine(
                    $"  Vote: elect yes {VillagePositionDisplay.GetSlug(position)} | elect no {VillagePositionDisplay.GetSlug(position)}");
            }
            else if (!holderId.HasValue)
            {
                builder.AppendLine($"  Run: run for {VillagePositionDisplay.GetSlug(position)}");
            }

            builder.AppendLine();
        }

        if (_economyService.TryGetState(playerEntityId, out var economy)
            && VillagePositionConfig.CanViewVillageOverview(economy.VillagePosition))
        {
            builder.AppendLine("Village overview (leadership privilege):");
            var summaries = await _playerRepository.GetContributionSummariesAsync();
            var totalContribution = summaries.Sum(summary => summary.VillageContributionScore);
            builder.AppendLine($"  Total recorded contribution: {totalContribution}");
            builder.AppendLine($"  Known villagers: {summaries.Count}");

            foreach (var summary in summaries.Take(8))
            {
                var positionLabel = summary.VillagePositionId == (int)VillagePosition.None
                    ? string.Empty
                    : $", {VillagePositionDisplay.GetName((VillagePosition)summary.VillagePositionId)}";
                builder.AppendLine(
                    $"  - Player {summary.EntityId}: {summary.VillageContributionScore} contribution ({VillageTitleDisplay.GetName((VillageTitle)summary.VillageTitleId)}{positionLabel})");
            }
        }

        var councilDashboard = await _councilService.BuildCouncilDashboardAsync(playerEntityId);
        if (!string.IsNullOrWhiteSpace(councilDashboard))
        {
            builder.AppendLine();
            builder.AppendLine(councilDashboard);
        }

        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.List,
            VillagePositionFailureReason.None,
            builder.ToString().TrimEnd());
    }

    private async Task<VillagePositionResponse> RunForPositionAsync(uint playerEntityId, VillagePosition position)
    {
        if (position == VillagePosition.None)
        {
            return Fail(
                VillagePositionRequestKind.RunFor,
                VillagePositionFailureReason.UnknownPosition,
                "Unknown position. Use: run for chief, deputy, advisor, or project-leader");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillagePositionRequestKind.RunFor,
                VillagePositionFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (economy.VillagePosition != VillagePosition.None)
        {
            return Fail(
                VillagePositionRequestKind.RunFor,
                VillagePositionFailureReason.AlreadyHoldingPosition,
                $"You already hold the {VillagePositionDisplay.GetName(economy.VillagePosition)} position.");
        }

        var holderId = await _playerRepository.GetHolderPlayerIdAsync(position);
        if (holderId.HasValue)
        {
            return Fail(
                VillagePositionRequestKind.RunFor,
                VillagePositionFailureReason.PositionFilled,
                $"{VillagePositionDisplay.GetName(position)} is currently held by player {holderId.Value}.");
        }

        if (!IsEligible(economy, position, out var eligibilityMessage))
        {
            return Fail(VillagePositionRequestKind.RunFor, VillagePositionFailureReason.NotEligible, eligibilityMessage);
        }

        var activeElection = await _electionRepository.GetActiveByPositionAsync(position);
        if (activeElection is not null)
        {
            return Fail(
                VillagePositionRequestKind.RunFor,
                VillagePositionFailureReason.ElectionInProgress,
                $"An election for {VillagePositionDisplay.GetName(position)} is already in progress (candidate player {activeElection.CandidatePlayerId}).");
        }

        var votingEndTotal = GameTimeMath.AddGameMinutes(
            _worldTime.GameDay,
            _worldTime.GameMinute,
            VillagePositionConfig.ElectionDurationGameMinutes);

        var electionId = await _electionRepository.InsertAsync(new PositionElectionRecord
        {
            Position = position,
            CandidatePlayerId = playerEntityId,
            Status = PositionElectionStatus.Voting,
            VotingEndTotalMinutes = votingEndTotal,
            CreatedAtUtc = DateTime.UtcNow,
        });

        Log.Information(
            "Player {PlayerId} ({Title}, score {Score}) started election #{ElectionId} for {Position}.",
            playerEntityId,
            VillageTitleDisplay.GetName(economy.VillageTitle),
            economy.VillageContributionScore,
            electionId,
            VillagePositionDisplay.GetName(position));

        var slug = VillagePositionDisplay.GetSlug(position);
        var message =
            $"Election started for {VillagePositionDisplay.GetName(position)} — candidate player {playerEntityId}. " +
            $"Vote: elect yes {slug} or elect no {slug}. Closes in {VillagePositionConfig.ElectionDurationGameMinutes} game minutes.";
        ElectionStarted?.Invoke(message);

        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.RunFor,
            VillagePositionFailureReason.None,
            $"You are now a candidate for {VillagePositionDisplay.GetName(position)} (election #{electionId}). Others can vote with: elect yes {slug}");
    }

    private async Task<VillagePositionResponse> CastElectionVoteAsync(
        uint playerEntityId,
        VillagePosition position,
        ProjectVoteChoice voteChoice)
    {
        if (voteChoice is not (ProjectVoteChoice.Yes or ProjectVoteChoice.No))
        {
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.InvalidVote,
                "Vote must be yes or no. Usage: elect yes chief");
        }

        if (position == VillagePosition.None)
        {
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.UnknownPosition,
                "Specify a position slug. Usage: elect yes chief");
        }

        var election = await _electionRepository.GetActiveByPositionAsync(position);
        if (election is null)
        {
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.NoActiveElection,
                $"No active election for {VillagePositionDisplay.GetName(position)}.");
        }

        var currentTotal = GetCurrentTotalMinutes();
        if (currentTotal >= election.VotingEndTotalMinutes)
        {
            await FinalizeElectionAsync(election);
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.VotingClosed,
                $"Election for {VillagePositionDisplay.GetName(position)} has ended.");
        }

        if (await _electionVoteRepository.HasVotedAsync(election.Id, playerEntityId))
        {
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.AlreadyVoted,
                $"You have already voted in the {VillagePositionDisplay.GetName(position)} election.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillagePositionRequestKind.Vote,
                VillagePositionFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        var voteWeight = VoteWeightCalculator.GetWeight(economy.VillageTitle);
        await _electionVoteRepository.InsertAsync(new PositionElectionVoteRecord
        {
            ElectionId = election.Id,
            PlayerEntityId = playerEntityId,
            Vote = voteChoice,
            VoteWeight = voteWeight,
            VotedAtUtc = DateTime.UtcNow,
        });

        var tally = await _electionVoteRepository.GetWeightedTallyAsync(election.Id);
        var voteLabel = voteChoice == ProjectVoteChoice.Yes ? "Yes" : "No";

        Log.Information(
            "Player {PlayerId} voted {Vote} (weight {VoteWeight}) in election #{ElectionId} for {Position}. Tally: {Tally}.",
            playerEntityId,
            voteLabel,
            voteWeight,
            election.Id,
            VillagePositionDisplay.GetName(position),
            tally.FormatCountsAndWeights());

        var remaining = GameTimeMath.MinutesRemaining(currentTotal, election.VotingEndTotalMinutes);
        return new VillagePositionResponse(
            true,
            VillagePositionRequestKind.Vote,
            VillagePositionFailureReason.None,
            $"You voted {voteLabel} for {VillagePositionDisplay.GetName(position)} (your vote weight: {voteWeight}). " +
            $"Current tally: {tally.FormatCountsAndWeights()}. {remaining} game minute(s) remaining.");
    }

    private async Task FinalizeExpiredElectionsAsync(long currentTotalMinutes)
    {
        var expired = await _electionRepository.GetExpiredVotingAsync(currentTotalMinutes);
        foreach (var election in expired)
            await FinalizeElectionAsync(election);
    }

    /// <summary>
    /// Weighted election outcome: Yes weight must exceed No weight with minimum distinct voters.
    /// </summary>
    private async Task FinalizeElectionAsync(PositionElectionRecord election)
    {
        if (election.Status != PositionElectionStatus.Voting)
            return;

        var tally = await _electionVoteRepository.GetWeightedTallyAsync(election.Id);
        var approved = tally.WouldApprove(VillagePositionConfig.MinimumVoterCount);
        var positionName = VillagePositionDisplay.GetName(election.Position);

        if (approved)
        {
            await _playerRepository.ClearVillagePositionAsync(election.Position);
            await AssignPositionAsync(election.CandidatePlayerId, election.Position);
            await _electionRepository.UpdateStatusAsync(election.Id, PositionElectionStatus.Approved);

            Log.Information(
                "Election #{ElectionId} APPROVED — player {PlayerId} is now {Position} ({Tally}).",
                election.Id,
                election.CandidatePlayerId,
                positionName,
                tally.FormatCountsAndWeights());

            ElectionEnded?.Invoke(
                $"Election ended — player {election.CandidatePlayerId} elected as {positionName} ({tally.FormatCountsAndWeights()}).");
        }
        else
        {
            await _electionRepository.UpdateStatusAsync(election.Id, PositionElectionStatus.Rejected);

            var reason = tally.TotalVoters < VillagePositionConfig.MinimumVoterCount
                ? $"only {tally.TotalVoters} voter(s), need {VillagePositionConfig.MinimumVoterCount}"
                : $"Yes weight {tally.YesWeight} did not exceed No weight {tally.NoWeight}";

            Log.Information(
                "Election #{ElectionId} REJECTED for {Position} ({Tally}) — {Reason}.",
                election.Id,
                positionName,
                tally.FormatCountsAndWeights(),
                reason);

            ElectionEnded?.Invoke(
                $"Election ended — {positionName} candidate player {election.CandidatePlayerId} was not elected ({tally.FormatCountsAndWeights()}). {reason}.");
        }
    }

    private async Task AssignPositionAsync(uint playerEntityId, VillagePosition position)
    {
        var assignedAt = DateTime.UtcNow;
        await _playerRepository.UpdateVillagePositionAsync(playerEntityId, position, assignedAt);

        if (_economyService.TryGetState(playerEntityId, out var economy))
        {
            economy.VillagePosition = position;
            economy.PositionAssignedAtUtc = assignedAt;
            await _economyService.SavePlayerAsync(playerEntityId);
        }

        Log.Information(
            "Player {PlayerId} assigned village position {Position}.",
            playerEntityId,
            VillagePositionDisplay.GetName(position));
    }

    private static bool IsEligible(PlayerEconomyState economy, VillagePosition position, out string message)
    {
        var requirement = VillagePositionConfig.GetRequirement(position);

        if (economy.VillageTitle < requirement.MinimumTitle)
        {
            message =
                $"Requires {VillageTitleDisplay.GetName(requirement.MinimumTitle)} title or higher. " +
                $"Your title: {VillageTitleDisplay.GetName(economy.VillageTitle)}.";
            return false;
        }

        if (economy.VillageContributionScore < requirement.MinimumContributionScore)
        {
            message =
                $"Requires at least {requirement.MinimumContributionScore} village contribution. " +
                $"Your score: {economy.VillageContributionScore}.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static IEnumerable<VillagePosition> GetLeadershipPositions()
    {
        yield return VillagePosition.ProjectLeader;
        yield return VillagePosition.Advisor;
        yield return VillagePosition.DeputyChief;
        yield return VillagePosition.Chief;
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
        Log.Information("Village position request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new VillagePositionResponse(false, kind, reason, message);
    }
}