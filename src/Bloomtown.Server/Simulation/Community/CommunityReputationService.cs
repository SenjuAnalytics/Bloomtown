using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Tracks community-help frequency, derives light social roles, and gates recurring NPC acknowledgment.
/// </summary>
public sealed class CommunityReputationService
{
    private readonly CommunityReputationRepository _repository;
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<uint, CommunityReputationState> _cache = new();
    private readonly Dictionary<uint, DateTime> _recurringHelpAcknowledgmentCooldowns = new();
    private readonly Dictionary<uint, DateTime> _ambientRoleCommentCooldowns = new();
    private readonly Dictionary<uint, long> _interactionRecognitionGameMinute = new();
    private readonly Dictionary<uint, uint> _ambientAttemptCounters = new();

    public CommunityReputationService(
        CommunityReputationRepository repository,
        WorldTimeSystem worldTime)
    {
        _repository = repository;
        _worldTime = worldTime;
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
        _recurringHelpAcknowledgmentCooldowns.Remove(playerEntityId);
        _ambientRoleCommentCooldowns.Remove(playerEntityId);
        _interactionRecognitionGameMinute.Remove(playerEntityId);
        _ambientAttemptCounters.Remove(playerEntityId);
    }

    public async Task LoadAsync(uint playerEntityId)
    {
        var record = await _repository.GetAsync(playerEntityId);
        _cache[playerEntityId] = record is null
            ? CommunityReputationConfig.CreateEmpty()
            : new CommunityReputationState(
                record.HelpGardenCount,
                record.HelpMarketCount,
                record.HelpWellCount);

        Log.Debug(
            "Loaded community reputation for player {PlayerId}: garden {Garden}, market {Market}, well {Well}, role {Role}.",
            playerEntityId,
            _cache[playerEntityId].HelpGardenCount,
            _cache[playerEntityId].HelpMarketCount,
            _cache[playerEntityId].HelpWellCount,
            CommunityReputationConfig.ResolveSocialRole(_cache[playerEntityId]));
    }

    public CommunityReputationState GetState(uint playerEntityId) =>
        _cache.TryGetValue(playerEntityId, out var state)
            ? state
            : CommunityReputationConfig.CreateEmpty();

    /// <summary>Current dominant social role derived from persisted help-frequency counters.</summary>
    public CommunitySocialRole GetDominantSocialRole(uint playerEntityId) =>
        CommunityReputationConfig.GetDominantSocialRole(GetState(playerEntityId));

    /// <summary>Whether the village has seen enough participation to surface role-based reactions.</summary>
    public bool IsEligibleForVillageReaction(uint playerEntityId) =>
        CommunityReputationConfig.IsEligibleForVillageReaction(GetState(playerEntityId));

    /// <summary>
    /// Increments help counters after a successful community activity and persists the new totals.
    /// </summary>
    public async Task<CommunityReputationState> RecordHelpAsync(uint playerEntityId, CommunityActivityKind activity)
    {
        var before = GetState(playerEntityId);
        var after = CommunityReputationConfig.Increment(before, activity);
        _cache[playerEntityId] = after;

        await _repository.UpsertAsync(new Persistence.Models.PlayerCommunityReputationRecord
        {
            PlayerEntityId = playerEntityId,
            HelpGardenCount = after.HelpGardenCount,
            HelpMarketCount = after.HelpMarketCount,
            HelpWellCount = after.HelpWellCount,
        });

        var roleBefore = CommunityReputationConfig.ResolveSocialRole(before);
        var roleAfter = CommunityReputationConfig.ResolveSocialRole(after);

        Log.Information(
            "Player {PlayerId} community reputation updated ({Activity}) — garden {GardenBefore}->{GardenAfter}, market {MarketBefore}->{MarketAfter}, well {WellBefore}->{WellAfter}, role {RoleBefore}->{RoleAfter}.",
            playerEntityId,
            activity,
            before.HelpGardenCount,
            after.HelpGardenCount,
            before.HelpMarketCount,
            after.HelpMarketCount,
            before.HelpWellCount,
            after.HelpWellCount,
            roleBefore,
            roleAfter);

        return after;
    }

    /// <summary>
    /// Rare recurring acknowledgment right after community help when frequency thresholds are met.
    /// </summary>
    public bool TryGetRecurringHelpAcknowledgment(
        uint playerEntityId,
        CommunityActivityKind activity,
        uint npcEntityId,
        uint variationSeed,
        out string line)
    {
        line = string.Empty;
        var state = GetState(playerEntityId);

        if (!CommunityReputationConfig.IsRightMomentForVillageReaction(
                state,
                VillageReactionSurface.RecurringHelpAcknowledgment))
        {
            return false;
        }

        if (CommunityReputationConfig.GetHelpCount(state, activity) < CommunityReputationConfig.MinHelpsForRecurringAcknowledgment)
            return false;

        if (!TryConsumeRecurringHelpAcknowledgmentCooldown(playerEntityId))
            return false;

        if (!CommunityReputationConfig.ShouldTriggerRecurringHelpAcknowledgment(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var acknowledgment = CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            activity,
            state,
            npcEntityId,
            variationSeed);

        if (string.IsNullOrWhiteSpace(acknowledgment))
            return false;

        line = acknowledgment;
        Log.Information(
            "Recurring community-help acknowledgment ({Activity}, role {Role}) from NPC {NpcId} to player {PlayerId}: \"{Line}\"",
            activity,
            CommunityReputationConfig.ResolveSocialRole(state),
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>Recurring social-role line during talk/greet — especially from Elsie.</summary>
    public bool TryGetInteractionRecognition(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        out string line)
    {
        line = string.Empty;
        var state = GetState(playerEntityId);
        var role = CommunityReputationConfig.ResolveSocialRole(state);

        if (!CommunityReputationConfig.IsRightMomentForVillageReaction(
                state,
                VillageReactionSurface.InteractionRecognition))
        {
            return false;
        }

        if (role == CommunitySocialRole.None)
            return false;

        if (!TryConsumeInteractionRecognitionCooldown(playerEntityId))
            return false;

        if (!CommunityReputationConfig.ShouldTriggerInteractionRecognition(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var recognition = CommunityReputationConfig.TryGetInteractionRecognition(state, npcEntityId, variationSeed);
        if (string.IsNullOrWhiteSpace(recognition))
            return false;

        line = recognition;
        Log.Information(
            "Recurring social-role interaction recognition ({Role}) from NPC {NpcId} to player {PlayerId}: \"{Line}\"",
            role,
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>Overheard village comment about the player's emerging social role.</summary>
    public bool TryGetAmbientRoleComment(
        uint playerEntityId,
        uint variationSeed,
        out string comment)
    {
        comment = string.Empty;
        var state = GetState(playerEntityId);
        var role = CommunityReputationConfig.ResolveSocialRole(state);

        if (!CommunityReputationConfig.IsRightMomentForVillageReaction(
                state,
                VillageReactionSurface.AmbientRoleComment))
        {
            return false;
        }

        if (role == CommunitySocialRole.None)
            return false;

        if (!TryConsumeAmbientRoleCommentCooldown(playerEntityId))
            return false;

        var attempt = _ambientAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _ambientAttemptCounters[playerEntityId] = attempt;

        if (!CommunityReputationConfig.ShouldTriggerAmbientRoleComment(
                playerEntityId,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        var line = CommunityReputationConfig.TryGetAmbientRoleComment(state, variationSeed);
        if (string.IsNullOrWhiteSpace(line))
            return false;

        comment = line;
        Log.Information(
            "Recurring social-role ambient comment ({Role}) to player {PlayerId}: \"{Comment}\"",
            role,
            playerEntityId,
            comment);

        return true;
    }

    public string? FormatParticipationStatus(uint playerEntityId) =>
        CommunityReputationConfig.FormatParticipationStatus(GetState(playerEntityId));

    private bool TryConsumeRecurringHelpAcknowledgmentCooldown(uint playerEntityId)
    {
        if (_recurringHelpAcknowledgmentCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < CommunityReputationConfig.RecurringHelpAcknowledgmentCooldown)
        {
            return false;
        }

        _recurringHelpAcknowledgmentCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    private bool TryConsumeAmbientRoleCommentCooldown(uint playerEntityId)
    {
        if (_ambientRoleCommentCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < CommunityReputationConfig.AmbientRoleCommentCooldown)
        {
            return false;
        }

        _ambientRoleCommentCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    private bool TryConsumeInteractionRecognitionCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;
        if (_interactionRecognitionGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < CommunityReputationConfig.InteractionRecognitionCooldownGameMinutes)
        {
            return false;
        }

        _interactionRecognitionGameMinute[playerEntityId] = currentMinute;
        return true;
    }
}