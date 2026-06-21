using Bloomtown.Shared.Relationship;

namespace Bloomtown.Server.Simulation.Relationship;

/// <summary>
/// Maps raw affinity points to a relationship tier.
/// </summary>
public static class RelationshipTierCalculator
{
    public static RelationshipTier GetTier(int affinityValue)
    {
        var value = Math.Clamp(affinityValue, 0, RelationshipConfig.MaxAffinity);

        if (value >= RelationshipConfig.CloseFriendThreshold)
            return RelationshipTier.CloseFriend;

        if (value >= RelationshipConfig.FriendThreshold)
            return RelationshipTier.Friend;

        if (value >= RelationshipConfig.AcquaintanceThreshold)
            return RelationshipTier.Acquaintance;

        return RelationshipTier.Stranger;
    }
}