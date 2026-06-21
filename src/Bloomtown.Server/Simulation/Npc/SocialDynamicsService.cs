using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Gates light NPC information, personal habit lines, and contextual ambient comments with cooldowns.
/// </summary>
public sealed class SocialDynamicsService
{
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<uint, DateTime> _lightInfoCooldowns = new();
    private readonly Dictionary<uint, DateTime> _personalHabitCooldowns = new();
    private readonly Dictionary<uint, DateTime> _contextualAmbientCooldowns = new();
    private readonly Dictionary<uint, uint> _contextualAmbientAttemptCounters = new();

    public SocialDynamicsService(WorldTimeSystem worldTime)
    {
        _worldTime = worldTime;
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _lightInfoCooldowns.Remove(playerEntityId);
        _personalHabitCooldowns.Remove(playerEntityId);
        _contextualAmbientCooldowns.Remove(playerEntityId);
        _contextualAmbientAttemptCounters.Remove(playerEntityId);
    }

    /// <summary>Rare personal habit line during greet/talk — shows the NPC knows the player's routines.</summary>
    public bool TryGetPersonalHabitResponse(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        uint variationSeed,
        out string line)
    {
        line = string.Empty;

        if (!SocialDynamicsConfig.IsInfoSharingNpc(npcEntityId))
            return false;

        if (!TryConsumePersonalHabitCooldown(playerEntityId))
            return false;

        if (!SocialDynamicsConfig.ShouldTriggerPersonalHabit(playerEntityId, _worldTime.TotalGameMinutes))
            return false;

        var habit = SocialDynamicsConfig.TryGetPersonalHabitLine(
            npcEntityId,
            tier,
            reputation,
            variationSeed);

        if (string.IsNullOrWhiteSpace(habit))
            return false;

        line = habit;
        Log.Information(
            "Personal habit recognition from {NpcId} to player {PlayerId} (tier {Tier}, role {Role}): \"{Line}\"",
            npcEntityId,
            playerEntityId,
            RelationshipTierDisplay.GetName(tier),
            CommunityReputationConfig.GetDominantSocialRole(reputation),
            line);

        return true;
    }

    /// <summary>Appends a light village tip after greet/talk — location hints, not quest-critical info.</summary>
    public bool TryGetLightInfoAppendix(
        uint playerEntityId,
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed,
        out string appendix)
    {
        appendix = string.Empty;

        if (!SocialDynamicsConfig.IsInfoSharingNpc(npcEntityId))
            return false;

        if (!TryConsumeLightInfoCooldown(playerEntityId))
            return false;

        if (!SocialDynamicsConfig.ShouldTriggerLightInfo(playerEntityId, _worldTime.TotalGameMinutes))
            return false;

        var tip = SocialDynamicsConfig.TryGetLightInfoTip(
            npcEntityId,
            timeOfDay,
            developmentLevel,
            completedProjectIds,
            variationSeed);

        if (string.IsNullOrWhiteSpace(tip))
            return false;

        appendix = tip;
        Log.Information(
            "Light social info from {NpcId} to player {PlayerId} ({TimeOfDay}): \"{Tip}\"",
            npcEntityId,
            playerEntityId,
            timeOfDay,
            tip);

        return true;
    }

    /// <summary>Contextual ambient comment shaped by player–NPC bond and recent village state.</summary>
    public bool TryGetContextualAmbientComment(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        NpcInterpersonalRelationship elsieTomRelationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed,
        out string comment)
    {
        comment = string.Empty;

        if (!TryConsumeContextualAmbientCooldown(playerEntityId))
            return false;

        var attempt = _contextualAmbientAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _contextualAmbientAttemptCounters[playerEntityId] = attempt;

        if (!SocialDynamicsConfig.ShouldTriggerContextualAmbient(
                playerEntityId,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        var line = SocialDynamicsConfig.TryGetContextualAmbientComment(
            npcEntityId,
            tier,
            reputation,
            elsieTomRelationship,
            timeOfDay,
            developmentLevel,
            completedProjectIds,
            variationSeed);

        if (string.IsNullOrWhiteSpace(line))
            return false;

        comment = line;
        Log.Information(
            "Contextual social ambient from {NpcId} to player {PlayerId} (tier {Tier}, role {Role}): \"{Comment}\"",
            npcEntityId,
            playerEntityId,
            RelationshipTierDisplay.GetName(tier),
            CommunityReputationConfig.GetDominantSocialRole(reputation),
            comment);

        return true;
    }

    private bool TryConsumeLightInfoCooldown(uint playerEntityId)
    {
        if (_lightInfoCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < SocialDynamicsConfig.LightInfoCooldown)
        {
            return false;
        }

        _lightInfoCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    private bool TryConsumePersonalHabitCooldown(uint playerEntityId)
    {
        if (_personalHabitCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < SocialDynamicsConfig.PersonalHabitCooldown)
        {
            return false;
        }

        _personalHabitCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    private bool TryConsumeContextualAmbientCooldown(uint playerEntityId)
    {
        if (_contextualAmbientCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < SocialDynamicsConfig.ContextualAmbientCooldown)
        {
            return false;
        }

        _contextualAmbientCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }
}