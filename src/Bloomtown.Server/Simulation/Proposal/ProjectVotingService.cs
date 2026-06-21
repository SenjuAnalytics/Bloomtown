using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Manages voting periods, weighted vote collection, and final approval/rejection of proposals.
/// </summary>
public sealed class ProjectVotingService : ISimulationSystem
{
    private readonly VillageProjectProposalRepository _proposalRepository;
    private readonly ProjectVoteRepository _voteRepository;
    private readonly CommunityProjectService _communityProjectService;
    private readonly PlayerEconomyService _economyService;
    private readonly WorldTimeSystem _worldTime;
    private long _lastCheckedTotalMinutes = -1;

    public event Action<string>? VotingStarted;
    public event Action<string>? VotingEnded;

    public ProjectVotingService(
        VillageProjectProposalRepository proposalRepository,
        ProjectVoteRepository voteRepository,
        CommunityProjectService communityProjectService,
        PlayerEconomyService economyService,
        WorldTimeSystem worldTime)
    {
        _proposalRepository = proposalRepository;
        _voteRepository = voteRepository;
        _communityProjectService = communityProjectService;
        _economyService = economyService;
        _worldTime = worldTime;
    }

    /// <summary>
    /// Finalizes any voting periods that expired while the server was offline.
    /// </summary>
    public async Task LoadAsync()
    {
        var currentTotal = GetCurrentTotalMinutes();
        _lastCheckedTotalMinutes = currentTotal;
        await FinalizeExpiredVotingsAsync(currentTotal);
    }

    public void Update(double deltaTimeSeconds)
    {
        var currentTotal = GetCurrentTotalMinutes();
        if (currentTotal == _lastCheckedTotalMinutes)
            return;

        _lastCheckedTotalMinutes = currentTotal;
        FinalizeExpiredVotingsAsync(currentTotal).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Moves a newly submitted proposal into an active voting period.
    /// </summary>
    public async Task StartVotingAsync(int proposalId, string projectName, string slug, uint proposerId)
    {
        var votingEndTotal = GameTimeMath.AddGameMinutes(
            _worldTime.GameDay,
            _worldTime.GameMinute,
            ProjectVotingConfig.DurationGameMinutes);

        await _proposalRepository.UpdateVotingStateAsync(proposalId, ProposalStatus.Voting, votingEndTotal);

        Log.Information(
            "Weighted voting started for proposal #{ProposalId} '{ProjectName}' ({Slug}) by player {ProposerId}. Ends at game minute index {VotingEndTotal}.",
            proposalId,
            projectName,
            slug,
            proposerId,
            votingEndTotal);

        var message =
            $"Voting started on proposal #{proposalId}: '{projectName}'. " +
            $"Vote: vote yes on {proposalId} or vote no on {proposalId}. " +
            $"Closes in {ProjectVotingConfig.DurationGameMinutes} game minutes. " +
            $"Weighted rules: Yes weight > No weight, at least {ProjectVotingConfig.MinimumVoterCount} voters. " +
            $"Vote weights: Newcomer/Helper 1, Builder 2, Respected Villager 3, Elder Candidate 4.";
        VotingStarted?.Invoke(message);
    }

    public async Task<ProjectProposalResponse> CastVoteAsync(
        uint playerEntityId,
        int proposalId,
        string projectName,
        ProjectVoteChoice voteChoice)
    {
        if (voteChoice is not (ProjectVoteChoice.Yes or ProjectVoteChoice.No))
        {
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.InvalidVote,
                "Vote must be 'yes' or 'no'. Usage: vote yes on 5");
        }

        var proposal = await ResolveProposalAsync(proposalId, projectName);
        if (proposal is null)
        {
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.UnknownProposal,
                "Unknown proposal. Use 'proposals' to see open votes or specify a proposal id/name.");
        }

        if (proposal.Status != ProposalStatus.Voting)
        {
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.NotInVoting,
                $"Proposal #{proposal.Id} '{proposal.ProjectName}' is not open for voting (status: {proposal.Status}).");
        }

        var currentTotal = GetCurrentTotalMinutes();
        if (proposal.VotingEndTotalMinutes.HasValue && currentTotal >= proposal.VotingEndTotalMinutes.Value)
        {
            await FinalizeVotingAsync(proposal);
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.VotingClosed,
                $"Voting on proposal #{proposal.Id} has ended.");
        }

        if (await _voteRepository.HasVotedAsync(proposal.Id, playerEntityId))
        {
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.AlreadyVoted,
                $"You have already voted on proposal #{proposal.Id}.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                ProjectProposalRequestKind.Vote,
                ProjectProposalFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        // Vote weight follows the player's current village title at cast time.
        var voteWeight = VoteWeightCalculator.GetWeight(economy.VillageTitle);
        var titleName = VillageTitleDisplay.GetName(economy.VillageTitle);

        await _voteRepository.InsertAsync(new ProjectVoteRecord
        {
            ProposalId = proposal.Id,
            PlayerEntityId = playerEntityId,
            Vote = voteChoice,
            VoteWeight = voteWeight,
            VotedAtUtc = DateTime.UtcNow,
        });

        var tally = await _voteRepository.GetWeightedTallyAsync(proposal.Id);
        var voteLabel = voteChoice == ProjectVoteChoice.Yes ? "Yes" : "No";

        Log.Information(
            "Player {PlayerId} ({Title}, weight {VoteWeight}) voted {Vote} on proposal #{ProposalId} '{ProjectName}'. Weighted tally: {Tally}.",
            playerEntityId,
            titleName,
            voteWeight,
            voteLabel,
            proposal.Id,
            proposal.ProjectName,
            tally.FormatCountsAndWeights());

        var remaining = proposal.VotingEndTotalMinutes.HasValue
            ? GameTimeMath.MinutesRemaining(currentTotal, proposal.VotingEndTotalMinutes.Value)
            : 0;

        return new ProjectProposalResponse(
            true,
            ProjectProposalRequestKind.Vote,
            ProjectProposalFailureReason.None,
            $"You voted {voteLabel} on proposal #{proposal.Id} '{proposal.ProjectName}' (your vote weight: {voteWeight} as {titleName}). " +
            $"Current tally: {tally.FormatCountsAndWeights()}. {remaining} game minute(s) remaining.");
    }

    public Task<WeightedVoteTally> GetWeightedTallyAsync(int proposalId)
    {
        return _voteRepository.GetWeightedTallyAsync(proposalId);
    }

    public int GetMinutesRemaining(VillageProjectProposalRecord proposal)
    {
        if (!proposal.VotingEndTotalMinutes.HasValue)
            return 0;

        return GameTimeMath.MinutesRemaining(GetCurrentTotalMinutes(), proposal.VotingEndTotalMinutes.Value);
    }

    /// <summary>
    /// Elder Candidates can preview the weighted outcome before voting closes.
    /// </summary>
    public string? GetElderProjectedOutcome(WeightedVoteTally tally)
    {
        if (tally.WouldApprove(ProjectVotingConfig.MinimumVoterCount))
            return "Projected outcome (Elder preview): would be APPROVED if voting ended now.";

        if (tally.TotalVoters < ProjectVotingConfig.MinimumVoterCount)
        {
            return
                $"Projected outcome (Elder preview): would be REJECTED — only {tally.TotalVoters} voter(s), need {ProjectVotingConfig.MinimumVoterCount}.";
        }

        return
            $"Projected outcome (Elder preview): would be REJECTED — Yes weight {tally.YesWeight} does not exceed No weight {tally.NoWeight}.";
    }

    private async Task FinalizeExpiredVotingsAsync(long currentTotalMinutes)
    {
        var expired = await _proposalRepository.GetExpiredVotingAsync(currentTotalMinutes);
        foreach (var proposal in expired)
            await FinalizeVotingAsync(proposal);
    }

    /// <summary>
    /// Tallies weighted votes and approves when Yes weight exceeds No weight with enough voters.
    /// </summary>
    private async Task FinalizeVotingAsync(VillageProjectProposalRecord proposal)
    {
        if (proposal.Status != ProposalStatus.Voting)
            return;

        var tally = await _voteRepository.GetWeightedTallyAsync(proposal.Id);

        // Weighted majority with a minimum distinct-voter threshold.
        var approved = tally.WouldApprove(ProjectVotingConfig.MinimumVoterCount);

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
                    $"Proposal #{proposal.Id} '{proposal.ProjectName}' weighted voting passed ({tally.FormatCountsAndWeights()}) but activation failed.";
                VotingEnded?.Invoke(message);
                return;
            }

            Log.Information(
                "Proposal #{ProposalId} '{ProjectName}' APPROVED after weighted voting ({Tally}, voters {TotalVoters}) — community project id {ProjectId}.",
                proposal.Id,
                proposal.ProjectName,
                tally.FormatCountsAndWeights(),
                tally.TotalVoters,
                activation.CreatedProjectId);

            message =
                $"Voting ended — proposal #{proposal.Id} '{proposal.ProjectName}' APPROVED ({tally.FormatCountsAndWeights()}). " +
                $"Now an active community project ({proposal.ProjectSlug}). Use 'projects' to contribute.";
        }
        else
        {
            await _proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.Rejected, null);

            var reason = tally.TotalVoters < ProjectVotingConfig.MinimumVoterCount
                ? $"only {tally.TotalVoters} voter(s), need at least {ProjectVotingConfig.MinimumVoterCount}"
                : $"Yes weight {tally.YesWeight} did not exceed No weight {tally.NoWeight}";

            Log.Information(
                "Proposal #{ProposalId} '{ProjectName}' REJECTED after weighted voting ({Tally}) — {Reason}.",
                proposal.Id,
                proposal.ProjectName,
                tally.FormatCountsAndWeights(),
                reason);

            message =
                $"Voting ended — proposal #{proposal.Id} '{proposal.ProjectName}' REJECTED ({tally.FormatCountsAndWeights()}). {reason}.";
        }

        VotingEnded?.Invoke(message);
    }

    private async Task<VillageProjectProposalRecord?> ResolveProposalAsync(int proposalId, string projectName)
    {
        if (proposalId > 0)
            return await _proposalRepository.GetByIdAsync(proposalId);

        if (!string.IsNullOrWhiteSpace(projectName))
            return await _proposalRepository.GetByProjectNameAsync(projectName);

        return null;
    }

    private long GetCurrentTotalMinutes()
    {
        return GameTimeMath.ToTotalGameMinutes(_worldTime.GameDay, _worldTime.GameMinute);
    }

    private static ProjectProposalResponse Fail(
        ProjectProposalRequestKind kind,
        ProjectProposalFailureReason reason,
        string message)
    {
        Log.Information("Project vote request failed ({Reason}): {Message}", reason, message);
        return new ProjectProposalResponse(false, kind, reason, message);
    }
}