using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Npc;

/// <summary>
/// Elsie–Tom relationship defaults, evolution rules, and display helpers.
/// </summary>
public static class NpcInterpersonalRelationshipConfig
{
    public const uint ElsieNpcEntityId = NpcEntityIds.Elsie;
    public const uint TomNpcEntityId = NpcEntityIds.Tom;

    /// <summary>Starting tone before shared village work deepens their rapport.</summary>
    public const NpcInterpersonalRelationship DefaultRelationship = NpcInterpersonalRelationship.Neutral;

    /// <summary>Game days before time-based warming (only if the village has finished at least one project).</summary>
    public const int TimeEvolutionMinGameDay = 7;

    /// <summary>
    /// Resolves the next relationship state from completed projects and game day.
    /// Evolution is one-way and rare: Neutral may become Friendly, never the reverse.
    /// </summary>
    public static NpcInterpersonalRelationship ResolveRelationship(
        NpcInterpersonalRelationship current,
        IReadOnlyCollection<byte> completedProjectIds,
        int gameDay)
    {
        if (current == NpcInterpersonalRelationship.Friendly)
            return current;

        if (TriggersProjectBond(completedProjectIds))
            return NpcInterpersonalRelationship.Friendly;

        if (gameDay >= TimeEvolutionMinGameDay && completedProjectIds.Count > 0)
            return NpcInterpersonalRelationship.Friendly;

        return NpcInterpersonalRelationship.Neutral;
    }

    /// <summary>
    /// Well, Bridge, and Warehouse completions are shared-work milestones that warm Elsie–Tom rapport.
    /// </summary>
    public static bool TriggersProjectBond(IReadOnlyCollection<byte> completedProjectIds) =>
        completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId)
        || completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId)
        || completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId);

    public static string GetDisplayName(NpcInterpersonalRelationship relationship) =>
        relationship switch
        {
            NpcInterpersonalRelationship.Friendly => "Friendly",
            _ => "Neutral",
        };

    /// <summary>Light status-line flavor for how Elsie and Tom regard each other socially.</summary>
    public static string FormatRelationshipStatus(NpcInterpersonalRelationship relationship) =>
        relationship switch
        {
            NpcInterpersonalRelationship.Friendly =>
                "Village social bonds: Elsie and Tom — friendly neighbors who keep Bloomtown's rhythm steady.",
            _ =>
                "Village social bonds: Elsie and Tom — practical neighbors who coordinate when work needs doing.",
        };

    public static bool IsElsieTomPair(uint npcEntityIdA, uint npcEntityIdB) =>
        (npcEntityIdA == ElsieNpcEntityId && npcEntityIdB == TomNpcEntityId)
        || (npcEntityIdA == TomNpcEntityId && npcEntityIdB == ElsieNpcEntityId);
}