namespace Bloomtown.Shared.Housing;

public static class HouseTierDisplay
{
    public static string GetName(HouseTier tier)
    {
        return tier switch
        {
            HouseTier.Improved => "Improved",
            HouseTier.Comfortable => "Comfortable",
            _ => "Basic",
        };
    }
}