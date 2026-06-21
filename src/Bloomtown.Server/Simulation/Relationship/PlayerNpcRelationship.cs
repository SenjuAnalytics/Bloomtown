using Bloomtown.Shared.Relationship;

namespace Bloomtown.Server.Simulation.Relationship;

/// <summary>
/// Runtime affinity state for one player–NPC pair.
/// </summary>
public sealed class PlayerNpcRelationship
{
    public required uint PlayerEntityId { get; init; }
    public required uint NpcEntityId { get; init; }
    public int AffinityValue { get; set; }
    public int LastInteractionGameDay { get; set; }
    public DateTime LastInteractionUtc { get; set; } = DateTime.UtcNow;

    public RelationshipTier Tier => RelationshipTierCalculator.GetTier(AffinityValue);
}