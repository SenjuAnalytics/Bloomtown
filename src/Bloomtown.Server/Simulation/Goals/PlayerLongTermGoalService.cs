using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Legacy;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Npc;
using Serilog;

namespace Bloomtown.Server.Simulation.Goals;

/// <summary>
/// Tracks the player's village legacy goal, detects their legacy archetype from play patterns,
/// reconciles milestone progress, and gates archetype-aware milestone feedback.
/// </summary>
public sealed class PlayerLongTermGoalService
{
    private readonly PlayerLongTermGoalRepository _repository;
    private readonly PlayerEconomyService _economyService;
    private readonly CommunityReputationService _reputationService;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerLegacyService _legacyService;
    private readonly CommunityProjectService _communityProjectService;
    private readonly WorldTimeSystem _worldTime;

    private readonly Dictionary<uint, PlayerLongTermGoalProgress> _cache = new();
    private readonly Dictionary<uint, PlayerLongTermGoalMilestone?> _pendingMilestoneFeedback = new();
    private readonly Dictionary<uint, long> _lastMilestoneInteractionGameMinute = new();
    private readonly Dictionary<uint, long> _lastMilestoneAmbientGameMinute = new();
    private readonly Dictionary<uint, long> _lastConnectorInfluenceGameMinute = new();
    private readonly Dictionary<uint, long> _lastConsciousFocusGameMinute = new();
    private readonly Dictionary<uint, long> _lastIdentityInteractionGameMinute = new();
    private readonly Dictionary<uint, long> _lastIdentityAmbientGameMinute = new();

    public PlayerLongTermGoalService(
        PlayerLongTermGoalRepository repository,
        PlayerEconomyService economyService,
        CommunityReputationService reputationService,
        PlayerNpcRelationshipService relationshipService,
        PlayerLegacyService legacyService,
        CommunityProjectService communityProjectService,
        WorldTimeSystem worldTime)
    {
        _repository = repository;
        _economyService = economyService;
        _reputationService = reputationService;
        _relationshipService = relationshipService;
        _legacyService = legacyService;
        _communityProjectService = communityProjectService;
        _worldTime = worldTime;
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
        _pendingMilestoneFeedback.Remove(playerEntityId);
        _lastMilestoneInteractionGameMinute.Remove(playerEntityId);
        _lastMilestoneAmbientGameMinute.Remove(playerEntityId);
        _lastConnectorInfluenceGameMinute.Remove(playerEntityId);
        _lastConsciousFocusGameMinute.Remove(playerEntityId);
        _lastIdentityInteractionGameMinute.Remove(playerEntityId);
        _lastIdentityAmbientGameMinute.Remove(playerEntityId);
    }

    public async Task LoadAsync(uint playerEntityId)
    {
        var record = await _repository.GetAsync(playerEntityId);
        _cache[playerEntityId] = record is null
            ? PlayerLongTermGoalConfig.CreateDefault()
            : new PlayerLongTermGoalProgress(
                (PlayerLongTermGoalKind)record.GoalKind,
                (PlayerLongTermGoalMilestone)record.HighestCompletedMilestone,
                record.GoalCompletedAtUtc,
                (LegacyArchetype)record.LegacyArchetype,
                new LegacyArchetypeInfluence(
                    record.BuilderInfluence,
                    record.CaretakerInfluence,
                    record.ConnectorInfluence),
                (LegacyArchetype)record.LegacyFocus);

        Log.Debug(
            "Loaded long-term goal for player {PlayerId}: milestone {Milestone}, archetype {Archetype}, focus {Focus}, influence B{Builder}/C{Caretaker}/N{Connector}, complete={Complete}.",
            playerEntityId,
            _cache[playerEntityId].HighestCompletedMilestone,
            _cache[playerEntityId].LegacyArchetype,
            _cache[playerEntityId].ActiveFocus,
            _cache[playerEntityId].Influence.BuilderPoints,
            _cache[playerEntityId].Influence.CaretakerPoints,
            _cache[playerEntityId].Influence.ConnectorPoints,
            _cache[playerEntityId].IsComplete);
    }

    public PlayerLongTermGoalProgress GetProgress(uint playerEntityId) =>
        _cache.TryGetValue(playerEntityId, out var progress)
            ? progress
            : PlayerLongTermGoalConfig.CreateDefault();

    public LegacyArchetype GetLegacyArchetype(uint playerEntityId) =>
        GetProgress(playerEntityId).LegacyArchetype;

    public PlayerLongTermGoalSnapshot BuildSnapshot(uint playerEntityId)
    {
        var reputation = _reputationService.GetState(playerEntityId);
        var legacyContext = _legacyService.BuildContext(playerEntityId);

        var villageTitle = VillageTitle.Newcomer;
        var contributionScore = 0;
        if (_economyService.TryGetState(playerEntityId, out var economy))
        {
            villageTitle = economy.VillageTitle;
            contributionScore = economy.VillageContributionScore;
        }

        var (acquaintanceCount, friendCount, closeFriendCount) = CountRelationshipTiers(playerEntityId);
        var completedProjects = _communityProjectService
            .GetPlayerCompletedProjectContributions(playerEntityId)
            .Count;

        return new PlayerLongTermGoalSnapshot(
            reputation.TotalHelpCount,
            CommunityReputationConfig.GetDominantSocialRole(reputation),
            villageTitle,
            contributionScore,
            friendCount,
            acquaintanceCount,
            closeFriendCount,
            completedProjects,
            legacyContext.HasRecognition);
    }

    /// <summary>
    /// Re-evaluates legacy archetype and milestone progress from reputation, contribution, and relationships.
    /// Persists changes and queues archetype-flavored feedback for newly completed milestones.
    /// </summary>
    public async Task<IReadOnlyList<PlayerLongTermGoalMilestone>> ReconcileAsync(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var progress))
            progress = PlayerLongTermGoalConfig.CreateDefault();

        var reputation = _reputationService.GetState(playerEntityId);
        var snapshot = BuildSnapshot(playerEntityId);
        var detectedArchetype = LegacyArchetypeConfig.ResolveDominantArchetype(
            snapshot,
            reputation,
            progress.Influence);

        var archetypeChanged = detectedArchetype != progress.LegacyArchetype;
        if (archetypeChanged)
        {
            if (progress.LegacyArchetype == LegacyArchetype.None && detectedArchetype != LegacyArchetype.None)
            {
                Log.Information(
                    "Player {PlayerId} legacy archetype detected: {Archetype} ({ArchetypeName}).",
                    playerEntityId,
                    detectedArchetype,
                    LegacyArchetypeConfig.GetDisplayName(detectedArchetype));
            }
            else if (detectedArchetype != LegacyArchetype.None)
            {
                Log.Information(
                    "Player {PlayerId} legacy archetype shifted: {Previous} -> {Current}.",
                    playerEntityId,
                    LegacyArchetypeConfig.GetDisplayName(progress.LegacyArchetype),
                    LegacyArchetypeConfig.GetDisplayName(detectedArchetype));
            }

            progress = progress with { LegacyArchetype = detectedArchetype };
            _cache[playerEntityId] = progress;
        }

        if (progress.IsComplete)
        {
            if (archetypeChanged)
                await PersistProgressAsync(playerEntityId, progress);

            return Array.Empty<PlayerLongTermGoalMilestone>();
        }

        var newlyCompleted = PlayerLongTermGoalConfig.EvaluateNewMilestones(progress, snapshot);
        var progressChanged = newlyCompleted.Count > 0 || archetypeChanged;

        if (newlyCompleted.Count > 0)
        {
            var latest = newlyCompleted[^1];
            var completedAt = latest == PlayerLongTermGoalMilestone.BloomtownLegacy
                ? DateTime.UtcNow
                : progress.GoalCompletedAtUtc;

            progress = new PlayerLongTermGoalProgress(
                progress.GoalKind,
                latest,
                completedAt,
                progress.LegacyArchetype,
                progress.Influence,
                progress.ActiveFocus);

            _cache[playerEntityId] = progress;
            _pendingMilestoneFeedback[playerEntityId] = latest;

            foreach (var milestone in newlyCompleted)
            {
                Log.Information(
                    "Player {PlayerId} reached long-term goal milestone {Milestone} ({MilestoneName}) as {Archetype}.",
                    playerEntityId,
                    milestone,
                    PlayerLongTermGoalConfig.GetMilestoneDisplayName(milestone),
                    LegacyArchetypeConfig.GetDisplayName(progress.LegacyArchetype));
            }

            if (progress.IsComplete)
            {
                Log.Information(
                    "Player {PlayerId} completed long-term goal {Goal} with {Archetype} legacy — Bloomtown remembers.",
                    playerEntityId,
                    progress.GoalKind,
                    LegacyArchetypeConfig.GetDisplayName(progress.LegacyArchetype));
            }
        }

        if (progressChanged)
            await PersistProgressAsync(playerEntityId, progress);

        return newlyCompleted;
    }

    /// <summary>Records caretaker/connector influence from community help, then reconciles archetype.</summary>
    public async Task<IReadOnlyList<PlayerLongTermGoalMilestone>> RecordCommunityHelpAndReconcileAsync(
        uint playerEntityId,
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var path = LegacyArchetypeAgencyConfig.GetInfluenceForCommunityActivity(activity);
        if (path is not null)
            await ApplyInfluenceGainAsync(playerEntityId, path.Value, variationSeed);

        return await ReconcileAsync(playerEntityId);
    }

    /// <summary>Records builder influence from project contribution, then reconciles archetype.</summary>
    public async Task<IReadOnlyList<PlayerLongTermGoalMilestone>> RecordProjectContributionAndReconcileAsync(
        uint playerEntityId,
        uint variationSeed)
    {
        await ApplyInfluenceGainAsync(
            playerEntityId,
            LegacyArchetypeAgencyConfig.GetInfluenceForProjectContribution(),
            variationSeed);

        return await ReconcileAsync(playerEntityId);
    }

    /// <summary>Records connector influence from greet/talk (cooldown-gated), then reconciles archetype.</summary>
    public async Task<IReadOnlyList<PlayerLongTermGoalMilestone>> RecordNpcInteractionAndReconcileAsync(
        uint playerEntityId,
        uint variationSeed)
    {
        if (TryConsumeConnectorInfluenceCooldown(playerEntityId))
        {
            await ApplyInfluenceGainAsync(
                playerEntityId,
                LegacyArchetypeAgencyConfig.GetInfluenceForNpcInteraction(),
                variationSeed);
        }

        return await ReconcileAsync(playerEntityId);
    }

    public float GetCaretakerCommunityHelpMoodBonus(uint playerEntityId, CommunityActivityKind activity)
    {
        var progress = GetProgress(playerEntityId);
        return LegacyArchetypeAgencyConfig.GetCaretakerHelpMoodBonus(
            progress.LegacyArchetype,
            progress.Influence,
            activity);
    }

    public string? TryGetCommunityHelpAgencyFeedback(
        uint playerEntityId,
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var progress = GetProgress(playerEntityId);
        return LegacyArchetypeAgencyConfig.TryGetCommunityHelpAgencyFeedback(
            progress.LegacyArchetype,
            progress.Influence,
            activity,
            variationSeed);
    }

    public string? TryGetProjectContributionAgencyFeedback(uint playerEntityId, uint variationSeed)
    {
        var progress = GetProgress(playerEntityId);
        return LegacyArchetypeAgencyConfig.TryGetProjectContributionAgencyFeedback(
            progress.LegacyArchetype,
            progress.Influence,
            variationSeed);
    }

    public string? TryGetConnectorSocialInsight(uint playerEntityId, uint variationSeed)
    {
        var progress = GetProgress(playerEntityId);
        return LegacyArchetypeAgencyConfig.TryGetConnectorSocialInsight(
            progress.LegacyArchetype,
            progress.Influence,
            variationSeed);
    }

    public string? TryGetInfluenceGainFeedback(
        uint playerEntityId,
        LegacyArchetype path,
        uint variationSeed) =>
        LegacyArchetypeAgencyConfig.TryGetInfluenceGainFeedback(
            path,
            playerEntityId,
            _worldTime.TotalGameMinutes,
            variationSeed);

    /// <summary>
    /// Personal narrative feedback when an action supports the player's detected archetype.
    /// Gated by chance — should feel like the village noticing a habit, not every click.
    /// </summary>
    public string? TryGetPersonalAlignedActionFeedback(
        uint playerEntityId,
        LegacyAlignedActionKind action,
        uint variationSeed)
    {
        var progress = GetProgress(playerEntityId);
        if (progress.LegacyArchetype == LegacyArchetype.None)
            return null;

        if (!PlayerLongTermGoalConfig.ShouldShowPersonalAlignedActionFeedback(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return null;
        }

        var line = PlayerLongTermGoalConfig.TryGetPersonalAlignedActionFeedback(
            progress.LegacyArchetype,
            action,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return null;

        Log.Information(
            "Personal aligned-action feedback ({Archetype}, {Action}) for player {PlayerId}: \"{Line}\"",
            progress.LegacyArchetype,
            action,
            playerEntityId,
            line);

        return line;
    }

    /// <summary>NPC-specific archetype recognition — Elsie, Harold, and Mira each speak differently.</summary>
    public bool TryGetNpcArchetypeRecognition(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        out string? line)
    {
        line = null;

        if (npcEntityId is not (NpcEntityIds.Elsie or NpcEntityIds.Harold or NpcEntityIds.Mira))
            return false;

        var progress = GetProgress(playerEntityId);
        if (!LegacyArchetypeFocusConfig.QualifiesForIdentityRecognition(
                progress.LegacyArchetype,
                progress.Influence))
        {
            return false;
        }

        if (!TryConsumeIdentityInteractionCooldown(playerEntityId))
            return false;

        if (!PlayerLongTermGoalConfig.ShouldTriggerNpcArchetypeRecognition(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        line = PlayerLongTermGoalConfig.TryGetNpcArchetypeRecognitionLine(
            npcEntityId,
            progress.LegacyArchetype,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return false;

        Log.Information(
            "NPC archetype recognition ({Archetype}) from {NpcId} to player {PlayerId}: \"{Line}\"",
            progress.LegacyArchetype,
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>
    /// Links the player's legacy archetype to a personal emotional bond with Elsie or Harold.
    /// Gated by chance and cooldown — warmth between identity and relationship, not every chat.
    /// </summary>
    public bool TryGetEmotionalArchetypeBond(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        NpcMemoryService memoryService,
        uint variationSeed,
        out string? line)
    {
        line = null;

        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return false;

        var progress = GetProgress(playerEntityId);
        if (progress.LegacyArchetype == LegacyArchetype.None || tier < NpcEmotionalBondConfig.MinEmotionalInteractionTier)
            return false;

        if (!memoryService.TryConsumeEmotionalArchetypeBondCooldown(playerEntityId, npcEntityId))
            return false;

        if (!NpcEmotionalBondConfig.ShouldTriggerArchetypeEmotionalBond(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            npcEntityId,
            progress.LegacyArchetype,
            tier,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return false;

        Log.Information(
            "Emotional archetype bond ({Archetype}) from {NpcId} to player {PlayerId}: \"{Line}\"",
            progress.LegacyArchetype,
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>
    /// More personal milestone acknowledgment from Elsie or Harold when a milestone was recently earned.
    /// </summary>
    public bool TryGetEmotionalMilestoneBond(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        out string? line)
    {
        line = null;

        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return false;

        if (!_pendingMilestoneFeedback.TryGetValue(playerEntityId, out var milestone)
            || milestone is null)
        {
            return false;
        }

        var progress = GetProgress(playerEntityId);
        line = NpcEmotionalBondConfig.TryGetEmotionalMilestoneBondLine(
            npcEntityId,
            milestone.Value,
            progress.LegacyArchetype,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return false;

        Log.Information(
            "Emotional milestone bond ({Milestone}, {Archetype}) from {NpcId} to player {PlayerId}: \"{Line}\"",
            milestone,
            progress.LegacyArchetype,
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>
    /// Archetype-flavored feedback after a player-initiated emotional bonding action with Elsie or Harold.
    /// </summary>
    public string? TryGetActiveBondingArchetypeFeedback(
        uint playerEntityId,
        EmotionalBondActionKind action,
        uint variationSeed)
    {
        var progress = GetProgress(playerEntityId);
        if (progress.LegacyArchetype == LegacyArchetype.None)
            return null;

        var hint = NpcEmotionalBondAgencyConfig.TryGetArchetypeBondingHint(
            progress.LegacyArchetype,
            action,
            variationSeed);

        if (string.IsNullOrWhiteSpace(hint))
            return null;

        Log.Debug(
            "Active bonding archetype feedback for player {PlayerId} ({Archetype}, {Action}): \"{Hint}\"",
            playerEntityId,
            progress.LegacyArchetype,
            action,
            hint);

        return hint;
    }

    /// <summary>
    /// Conscious legacy-focus action: deliberate choice at the right location that strengthens one path
    /// (+2 influence) with light drift on others (-1). Persists active focus for status hints.
    /// </summary>
    public async Task<LegacyFocusResponse> PerformConsciousFocusAsync(
        uint playerEntityId,
        float playerX,
        float playerZ,
        bool hasNearbyNpc,
        LegacyFocusRequest request,
        uint variationSeed)
    {
        if (request.Kind != LegacyFocusRequestKind.Perform)
        {
            return FailFocus(
                request.Kind,
                LegacyFocusFailureReason.UnknownRequest,
                "Unknown legacy focus request.");
        }

        var path = request.Path;
        if (!LegacyArchetypeFocusConfig.IsValidFocusPath(path))
        {
            return FailFocus(
                request.Kind,
                LegacyFocusFailureReason.InvalidPath,
                "Choose a legacy path: focus build | focus tend | focus connect.");
        }

        if (!LegacyArchetypeFocusConfig.MeetsLocationRequirement(path, playerX, playerZ, hasNearbyNpc))
        {
            return FailFocus(
                request.Kind,
                LegacyFocusFailureReason.NotInRange,
                $"You need to be at the right place for {LegacyArchetypeConfig.GetDisplayName(path)} focus. "
                + LegacyArchetypeFocusConfig.FormatLocationHint(path));
        }

        if (!TryConsumeConsciousFocusCooldown(playerEntityId))
        {
            return FailFocus(
                request.Kind,
                LegacyFocusFailureReason.OnCooldown,
                $"Take a breath — conscious legacy choices need a little time ({LegacyArchetypeFocusConfig.FocusActionCooldownGameMinutes} game minutes).");
        }

        if (!_cache.TryGetValue(playerEntityId, out var progress))
            progress = PlayerLongTermGoalConfig.CreateDefault();

        var beforeInfluence = progress.Influence;
        var updatedInfluence = LegacyArchetypeFocusConfig.ApplyConsciousInfluenceGain(progress.Influence, path);
        progress = progress with
        {
            Influence = updatedInfluence,
            ActiveFocus = path,
        };
        _cache[playerEntityId] = progress;
        await PersistProgressAsync(playerEntityId, progress);

        Log.Information(
            "Player {PlayerId} conscious legacy focus on {Path}: influence B{BuilderBefore}->{BuilderAfter}, C{CaretakerBefore}->{CaretakerAfter}, N{ConnectorBefore}->{ConnectorAfter}.",
            playerEntityId,
            LegacyArchetypeConfig.GetDisplayName(path),
            beforeInfluence.BuilderPoints,
            updatedInfluence.BuilderPoints,
            beforeInfluence.CaretakerPoints,
            updatedInfluence.CaretakerPoints,
            beforeInfluence.ConnectorPoints,
            updatedInfluence.ConnectorPoints);

        await ReconcileAsync(playerEntityId);

        var updatedProgress = GetProgress(playerEntityId);
        var message = BuildConsciousFocusMessage(
            path,
            updatedProgress.LegacyArchetype,
            updatedProgress.Influence,
            variationSeed);

        Log.Information(
            "Conscious legacy focus feedback for player {PlayerId} ({Path}): \"{Message}\"",
            playerEntityId,
            path,
            message);

        return new LegacyFocusResponse(true, request.Kind, LegacyFocusFailureReason.None, message);
    }

    /// <summary>Personal NPC recognition when the village has named the player's legacy identity.</summary>
    public bool TryGetLegacyIdentityRecognition(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        out string? line) =>
        TryGetNpcArchetypeRecognition(playerEntityId, npcEntityId, variationSeed, out line);

    /// <summary>Overheard village comment recognizing the player's legacy identity.</summary>
    public bool TryGetLegacyIdentityAmbientFeedback(
        uint playerEntityId,
        uint variationSeed,
        out string comment,
        out uint speakerEntityId)
    {
        comment = string.Empty;
        speakerEntityId = 0;

        var progress = GetProgress(playerEntityId);
        if (!LegacyArchetypeFocusConfig.QualifiesForIdentityRecognition(
                progress.LegacyArchetype,
                progress.Influence))
        {
            return false;
        }

        if (!TryConsumeIdentityAmbientCooldown(playerEntityId))
            return false;

        if (!LegacyArchetypeFocusConfig.ShouldTriggerIdentityAmbient(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var line = LegacyArchetypeFocusConfig.TryGetIdentityAmbientLine(
            progress.LegacyArchetype,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return false;

        comment = line;
        speakerEntityId = progress.LegacyArchetype == LegacyArchetype.Connector
            ? NpcEntityIds.Mira
            : progress.LegacyArchetype == LegacyArchetype.Caretaker
                ? NpcEntityIds.Elsie
                : NpcEntityIds.Harold;

        Log.Information(
            "Legacy identity ambient ({Archetype}) to player {PlayerId}: \"{Comment}\"",
            progress.LegacyArchetype,
            playerEntityId,
            comment);

        return true;
    }

    public string FormatGoalStatusLine(uint playerEntityId)
    {
        var progress = GetProgress(playerEntityId);
        var snapshot = BuildSnapshot(playerEntityId);
        return PlayerLongTermGoalConfig.FormatGoalStatusLine(progress, snapshot);
    }

    public string FormatGoalDetail(uint playerEntityId)
    {
        var progress = GetProgress(playerEntityId);
        var snapshot = BuildSnapshot(playerEntityId);
        return PlayerLongTermGoalConfig.FormatGoalDetail(progress, snapshot);
    }

    /// <summary>Consumes pending milestone feedback for immediate action responses.</summary>
    public bool TryConsumePendingMilestoneFeedback(
        uint playerEntityId,
        uint variationSeed,
        out string? feedback)
    {
        feedback = null;

        if (!_pendingMilestoneFeedback.TryGetValue(playerEntityId, out var milestone)
            || milestone is null)
        {
            return false;
        }

        var archetype = GetLegacyArchetype(playerEntityId);
        feedback = PlayerLongTermGoalConfig.TryGetMilestoneFeedbackLine(
            milestone.Value,
            archetype,
            variationSeed);
        _pendingMilestoneFeedback.Remove(playerEntityId);

        if (string.IsNullOrWhiteSpace(feedback))
            return false;

        Log.Information(
            "Delivered pending milestone feedback ({Milestone}, {Archetype}) to player {PlayerId}: \"{Feedback}\"",
            milestone,
            archetype,
            playerEntityId,
            feedback);

        return true;
    }

    /// <summary>Milestone recognition during greet/talk — Elsie, Harold, or Mira speak for the village.</summary>
    public bool TryGetMilestoneInteractionFeedback(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        out string? line)
    {
        line = null;

        if (npcEntityId is not (NpcEntityIds.Elsie or NpcEntityIds.Harold or NpcEntityIds.Mira))
            return false;

        if (!_pendingMilestoneFeedback.TryGetValue(playerEntityId, out var milestone)
            || milestone is null)
        {
            return false;
        }

        if (!TryConsumeMilestoneInteractionCooldown(playerEntityId))
            return false;

        if (!PlayerLongTermGoalConfig.ShouldTriggerMilestoneInteraction(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var archetype = GetLegacyArchetype(playerEntityId);
        line = PlayerLongTermGoalConfig.TryGetMilestoneFeedbackLine(milestone.Value, archetype, variationSeed);
        if (string.IsNullOrWhiteSpace(line))
            return false;

        _pendingMilestoneFeedback.Remove(playerEntityId);

        Log.Information(
            "Milestone interaction feedback ({Milestone}, {Archetype}) from {NpcId} to player {PlayerId}: \"{Line}\"",
            milestone,
            archetype,
            npcEntityId,
            playerEntityId,
            line);

        return true;
    }

    /// <summary>Overheard village reaction when a milestone was recently earned.</summary>
    public bool TryGetMilestoneAmbientFeedback(
        uint playerEntityId,
        uint variationSeed,
        out string comment,
        out uint speakerEntityId)
    {
        comment = string.Empty;
        speakerEntityId = 0;

        if (!_pendingMilestoneFeedback.TryGetValue(playerEntityId, out var milestone)
            || milestone is null)
        {
            return false;
        }

        if (!TryConsumeMilestoneAmbientCooldown(playerEntityId))
            return false;

        if (!PlayerLongTermGoalConfig.ShouldTriggerMilestoneAmbient(
                playerEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var archetype = GetLegacyArchetype(playerEntityId);
        var line = PlayerLongTermGoalConfig.TryGetMilestoneAmbientLine(milestone.Value, archetype, variationSeed);
        if (string.IsNullOrWhiteSpace(line))
            return false;

        comment = line;
        speakerEntityId = archetype == LegacyArchetype.Connector
            ? NpcEntityIds.Mira
            : NpcEntityIds.Elsie;
        _pendingMilestoneFeedback.Remove(playerEntityId);

        Log.Information(
            "Milestone ambient feedback ({Milestone}, {Archetype}) to player {PlayerId}: \"{Comment}\"",
            milestone,
            archetype,
            playerEntityId,
            comment);

        return true;
    }

    private async Task PersistProgressAsync(uint playerEntityId, PlayerLongTermGoalProgress progress)
    {
        await _repository.UpsertAsync(new PlayerLongTermGoalRecord
        {
            PlayerEntityId = playerEntityId,
            GoalKind = (int)progress.GoalKind,
            HighestCompletedMilestone = (int)progress.HighestCompletedMilestone,
            GoalCompletedAtUtc = progress.GoalCompletedAtUtc,
            LegacyArchetype = (int)progress.LegacyArchetype,
            BuilderInfluence = progress.Influence.BuilderPoints,
            CaretakerInfluence = progress.Influence.CaretakerPoints,
            ConnectorInfluence = progress.Influence.ConnectorPoints,
            LegacyFocus = (int)progress.ActiveFocus,
        });
    }

    private static LegacyFocusResponse FailFocus(
        LegacyFocusRequestKind kind,
        LegacyFocusFailureReason reason,
        string message) =>
        new(false, kind, reason, message);

    private static string BuildConsciousFocusMessage(
        LegacyArchetype path,
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        uint variationSeed)
    {
        var builder = new System.Text.StringBuilder();
        var actionFeedback = LegacyArchetypeFocusConfig.TryGetConsciousActionFeedback(path, variationSeed);
        if (!string.IsNullOrWhiteSpace(actionFeedback))
            builder.Append(actionFeedback);

        var tradeoff = LegacyArchetypeFocusConfig.TryGetFocusTradeoffHint(path, variationSeed + 7);
        if (!string.IsNullOrWhiteSpace(tradeoff))
        {
            if (builder.Length > 0)
                builder.Append(' ');
            builder.Append(tradeoff);
        }

        var pathName = LegacyArchetypeConfig.GetDisplayName(path);
        var points = influence.GetPoints(path);
        if (builder.Length > 0)
            builder.Append($" Your {pathName} legacy influence is now {points}/{LegacyArchetypeAgencyConfig.MaxInfluencePerPath}.");
        else
            builder.Append($"You consciously strengthen your {pathName} legacy ({points}/{LegacyArchetypeAgencyConfig.MaxInfluencePerPath}).");

        var directionHint = LegacyArchetypeAgencyConfig.FormatLegacyDirectionHint(
            detectedArchetype,
            influence,
            variationSeed + 13);
        if (!string.IsNullOrWhiteSpace(directionHint))
        {
            builder.Append(' ');
            builder.Append(directionHint);
        }

        var personalFocus = PlayerLongTermGoalConfig.TryGetPersonalAlignedActionFeedback(
            detectedArchetype != LegacyArchetype.None ? detectedArchetype : path,
            LegacyAlignedActionKind.ConsciousFocus,
            variationSeed + 17);
        if (!string.IsNullOrWhiteSpace(personalFocus))
        {
            builder.Append(' ');
            builder.Append(personalFocus);
        }

        return builder.ToString().Trim();
    }

    private async Task ApplyInfluenceGainAsync(
        uint playerEntityId,
        LegacyArchetype path,
        uint variationSeed)
    {
        if (!_cache.TryGetValue(playerEntityId, out var progress))
            progress = PlayerLongTermGoalConfig.CreateDefault();

        var before = progress.Influence.GetPoints(path);
        var updatedInfluence = LegacyArchetypeAgencyConfig.ApplyInfluenceGain(progress.Influence, path);
        var after = updatedInfluence.GetPoints(path);

        if (after <= before)
            return;

        progress = progress with { Influence = updatedInfluence };
        _cache[playerEntityId] = progress;
        await PersistProgressAsync(playerEntityId, progress);

        Log.Information(
            "Player {PlayerId} legacy influence +{Path} ({Before}->{After}) via aligned action.",
            playerEntityId,
            LegacyArchetypeConfig.GetDisplayName(path),
            before,
            after);

        var gainFeedback = LegacyArchetypeAgencyConfig.TryGetInfluenceGainFeedback(
            path,
            playerEntityId,
            _worldTime.TotalGameMinutes,
            variationSeed);

        if (!string.IsNullOrWhiteSpace(gainFeedback))
        {
            Log.Information(
                "Legacy influence feedback for player {PlayerId} ({Path}): \"{Feedback}\"",
                playerEntityId,
                path,
                gainFeedback);
        }
    }

    private bool TryConsumeConsciousFocusCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastConsciousFocusGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < LegacyArchetypeFocusConfig.FocusActionCooldownGameMinutes)
        {
            return false;
        }

        _lastConsciousFocusGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private bool TryConsumeIdentityInteractionCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastIdentityInteractionGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < LegacyArchetypeFocusConfig.IdentityInteractionCooldownGameMinutes)
        {
            return false;
        }

        _lastIdentityInteractionGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private bool TryConsumeIdentityAmbientCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastIdentityAmbientGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < LegacyArchetypeFocusConfig.IdentityAmbientCooldownGameMinutes)
        {
            return false;
        }

        _lastIdentityAmbientGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private bool TryConsumeConnectorInfluenceCooldown(uint playerEntityId)
    {
        const int cooldownMinutes = 20;
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastConnectorInfluenceGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < cooldownMinutes)
        {
            return false;
        }

        _lastConnectorInfluenceGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private (int AcquaintanceCount, int FriendCount, int CloseFriendCount) CountRelationshipTiers(
        uint playerEntityId)
    {
        var acquaintance = 0;
        var friend = 0;
        var closeFriend = 0;

        foreach (var relationship in _relationshipService.GetRelationships(playerEntityId))
        {
            var tier = RelationshipTierCalculator.GetTier(relationship.AffinityValue);
            switch (tier)
            {
                case RelationshipTier.CloseFriend:
                    closeFriend++;
                    break;
                case RelationshipTier.Friend:
                    friend++;
                    break;
                case RelationshipTier.Acquaintance:
                    acquaintance++;
                    break;
            }
        }

        return (acquaintance, friend, closeFriend);
    }

    private bool TryConsumeMilestoneInteractionCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastMilestoneInteractionGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < PlayerLongTermGoalConfig.MilestoneInteractionCooldownGameMinutes)
        {
            return false;
        }

        _lastMilestoneInteractionGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private bool TryConsumeMilestoneAmbientCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastMilestoneAmbientGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < PlayerLongTermGoalConfig.MilestoneAmbientCooldownGameMinutes)
        {
            return false;
        }

        _lastMilestoneAmbientGameMinute[playerEntityId] = currentMinute;
        return true;
    }
}