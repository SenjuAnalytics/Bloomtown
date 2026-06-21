namespace Bloomtown.Shared.Relationship;

public static class RelationshipTierDisplay
{
    public static string GetName(RelationshipTier tier)
    {
        return tier switch
        {
            RelationshipTier.Acquaintance => "Acquaintance",
            RelationshipTier.Friend => "Friend",
            RelationshipTier.CloseFriend => "Close Friend",
            _ => "Stranger",
        };
    }
}