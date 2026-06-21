using Bloomtown.Server.Simulation.Contribution;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Adjusts shop prices based on player–NPC affinity.
/// Higher affinity: cheaper buys and better sell prices for the player.
/// </summary>
public static class ShopPriceCalculator
{
    public static int GetBuyPrice(int basePrice, int affinity)
    {
        return GetBuyPrice(basePrice, affinity, VillageTitle.Newcomer);
    }

    public static int GetBuyPrice(int basePrice, int affinity, VillageTitle villageTitle)
    {
        return GetBuyPrice(basePrice, affinity, villageTitle, merchantNpcEntityId: 0);
    }

    public static int GetBuyPrice(
        int basePrice,
        int affinity,
        VillageTitle villageTitle,
        uint merchantNpcEntityId,
        VillageSocialStandingTier standingTier = VillageSocialStandingTier.Stranger)
    {
        var clampedAffinity = Math.Clamp(affinity, 0, RelationshipConfig.MaxAffinity);
        var affinityMultiplier = 1.25f - clampedAffinity / 100f * 0.25f;
        var titleMultiplier = VillageContributionService.GetBuyPriceMultiplier(villageTitle);

        if (merchantNpcEntityId == NpcEntityIds.Mira
            && VillageSocialStandingMechanicalConfig.IsEligibleForMiraTradeBonus(standingTier))
        {
            affinityMultiplier *= VillageSocialStandingMechanicalConfig.GetMiraBuyPriceMultiplier(standingTier);
        }

        return Math.Max(1, (int)MathF.Round(basePrice * affinityMultiplier * titleMultiplier));
    }

    public static int GetSellPrice(int baseSellPrice, int affinity)
    {
        return GetSellPrice(baseSellPrice, affinity, merchantNpcEntityId: 0);
    }

    public static int GetSellPrice(
        int baseSellPrice,
        int affinity,
        uint merchantNpcEntityId,
        VillageSocialStandingTier standingTier = VillageSocialStandingTier.Stranger)
    {
        var clampedAffinity = Math.Clamp(affinity, 0, RelationshipConfig.MaxAffinity);
        var multiplier = 0.55f + clampedAffinity / 100f * 0.25f;

        if (merchantNpcEntityId == NpcEntityIds.Mira
            && VillageSocialStandingMechanicalConfig.IsEligibleForMiraTradeBonus(standingTier))
        {
            multiplier *= VillageSocialStandingMechanicalConfig.GetMiraSellPriceMultiplier(standingTier);
        }

        return Math.Max(1, (int)MathF.Round(baseSellPrice * multiplier));
    }

    public static int GetTotalBuyCost(int unitPrice, int quantity)
    {
        return unitPrice * quantity;
    }

    public static int GetTotalSellPayout(int unitPrice, int quantity)
    {
        return unitPrice * quantity;
    }
}