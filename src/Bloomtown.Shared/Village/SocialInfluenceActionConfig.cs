using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Active social-influence actions for Respected and Well-liked players calling on focus NPCs.
/// Respected tier grants limited outcomes with longer cooldowns; Well-liked unlocks full power.
/// </summary>
public static class SocialInfluenceActionConfig
{
    public const int RespectedSuccessChancePercent = 74;
    public const int SuccessChancePercent = 75;

    public const int RespectedCooldownGameMinutes = 152;

    public const int HaroldCooldownGameMinutes = 150;
    public const int GretaCooldownGameMinutes = 140;
    public const int MiraCooldownGameMinutes = 145;
    public const int ElsieCooldownGameMinutes = 145;
    public const int TomCooldownGameMinutes = 145;
    public const int NoraCooldownGameMinutes = 140;
    public const int EliasCooldownGameMinutes = 150;
    public const int BenCooldownGameMinutes = 150;
    public const int LilaCooldownGameMinutes = 150;
    public const int RowanCooldownGameMinutes = 145;
    public const int MarcusCooldownGameMinutes = 150;
    public const int EleanorCooldownGameMinutes = 145;

    public const int RespectedBackingProgressBonus = 1;
    public const int WellLikedBackingProgressBonus = 2;
    public const int BackingProgressBonus = WellLikedBackingProgressBonus;
    public const int RespectedActivityBonus = 1;
    public const int EliasSmithingActivityWoodBonus = 2;
    public const int BenPatrolActivityWoodBonus = 2;
    public const int LilaVillageActivityAppleBonus = 2;
    public const int RowanStoryActivityWoodBonus = 2;
    public const int MarcusCraftingActivityPlankBonus = 2;
    public const int EleanorLegacyActivityAppleBonus = 2;

    public const int RespectedInfoOutcomeWeight = 35;
    public const int RespectedBackingOutcomeWeight = 45;
    public const int RespectedSmallBonusOutcomeWeight = 20;

    public const int RespectedGretaInfoOutcomeWeight = 58;
    public const int RespectedGretaRecoveryOutcomeWeight = 22;
    public const int RespectedGretaItemOutcomeWeight = 20;

    public const int RespectedMiraInfoOutcomeWeight = 78;
    public const int RespectedMiraItemOutcomeWeight = 22;

    public const int RespectedNoraInfoOutcomeWeight = 38;
    public const int RespectedNoraHerbalBackingOutcomeWeight = 40;
    public const int RespectedNoraRecoveryOutcomeWeight = 12;
    public const int RespectedNoraItemOutcomeWeight = 20;

    public const int RespectedRowanInfoOutcomeWeight = 38;
    public const int RespectedRowanStoryBackingOutcomeWeight = 40;
    public const int RespectedRowanRecoveryOutcomeWeight = 12;
    public const int RespectedRowanItemOutcomeWeight = 20;

    public const int HaroldInfoOutcomeWeight = 40;
    public const int HaroldProjectBackingOutcomeWeight = 60;

    public const int GretaInfoOutcomeWeight = 35;
    public const int GretaRecoveryOutcomeWeight = 40;
    public const int GretaItemOutcomeWeight = 25;

    public const int MiraInfoOutcomeWeight = 40;
    public const int MiraTradePrivilegeOutcomeWeight = 40;
    public const int MiraItemOutcomeWeight = 20;

    public const int ElsieInfoOutcomeWeight = 40;
    public const int ElsieGardenBackingOutcomeWeight = 45;
    public const int ElsieItemOutcomeWeight = 15;

    public const int TomInfoOutcomeWeight = 40;
    public const int TomLumberBackingOutcomeWeight = 45;
    public const int TomItemOutcomeWeight = 15;

    public const int NoraInfoOutcomeWeight = 40;
    public const int NoraHerbalBackingOutcomeWeight = 45;
    public const int NoraItemOutcomeWeight = 15;

    public const int EliasInfoOutcomeWeight = 40;
    public const int EliasSmithingBackingOutcomeWeight = 45;
    public const int EliasItemOutcomeWeight = 15;

    public const int BenInfoOutcomeWeight = 40;
    public const int BenGuardBackingOutcomeWeight = 45;
    public const int BenItemOutcomeWeight = 15;

    public const int LilaInfoOutcomeWeight = 40;
    public const int LilaYouthBackingOutcomeWeight = 45;
    public const int LilaItemOutcomeWeight = 15;

    public const int RowanInfoOutcomeWeight = 40;
    public const int RowanStoryBackingOutcomeWeight = 45;
    public const int RowanItemOutcomeWeight = 15;

    public const int MarcusInfoOutcomeWeight = 40;
    public const int MarcusCraftingBackingOutcomeWeight = 45;
    public const int MarcusItemOutcomeWeight = 15;

    public const int EleanorInfoOutcomeWeight = 40;
    public const int EleanorLegacyBackingOutcomeWeight = 45;
    public const int EleanorItemOutcomeWeight = 15;

    public const int ItemGrantBaseQuantity = 3;
    public const int ItemGrantGenerousQuantity = 4;
    public const int RareItemGrantChancePercent = 22;

    public const float GretaMoodRecovery = 14f;
    public const float GretaSocialRecovery = 16f;

    public const float RespectedGretaMoodRecovery = 7f;
    public const float RespectedGretaSocialRecovery = 8f;

    public const float RespectedNoraMoodRecovery = 5f;
    public const float RespectedNoraSocialRecovery = 5f;

    public const float RespectedRowanMoodRecovery = 5f;
    public const float RespectedRowanSocialRecovery = 5f;

    public const int RespectedItemGrantQuantity = 1;
    public const int RespectedItemGrantGenerousQuantity = 2;

    public const float MiraTradePrivilegeBuyDiscountPercent = 10f;
    public const float MiraTradePrivilegeSellBonusPercent = 10f;
    public const int MiraTradePrivilegeTransactionCount = 2;

    public static readonly uint[] SupportedNpcEntityIds =
    [
        NpcEntityIds.Harold,
        NpcEntityIds.Greta,
        NpcEntityIds.Mira,
        NpcEntityIds.Elsie,
        NpcEntityIds.Tom,
        NpcEntityIds.Nora,
        NpcEntityIds.Elias,
        NpcEntityIds.Ben,
        NpcEntityIds.Lila,
        NpcEntityIds.Rowan,
        NpcEntityIds.Marcus,
        NpcEntityIds.Eleanor,
    ];

    public static bool IsEligible(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Respected;

    public static bool IsWellLikedTier(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked;

    public static int GetSuccessChancePercent(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? SuccessChancePercent
            : RespectedSuccessChancePercent;

    public static int GetBackingProgressBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? WellLikedBackingProgressBonus
            : RespectedBackingProgressBonus;

    public static int GetActivityBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? WellLikedBackingProgressBonus
            : RespectedActivityBonus;

    public static bool IsSupportedNpc(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Harold
            or NpcEntityIds.Greta
            or NpcEntityIds.Mira
            or NpcEntityIds.Elsie
            or NpcEntityIds.Tom
            or NpcEntityIds.Nora
            or NpcEntityIds.Elias
            or NpcEntityIds.Ben
            or NpcEntityIds.Lila
            or NpcEntityIds.Rowan
            or NpcEntityIds.Marcus
            or NpcEntityIds.Eleanor;

    public static int GetCooldownGameMinutes(uint npcEntityId, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? GetWellLikedCooldownGameMinutes(npcEntityId)
            : RespectedCooldownGameMinutes;

    public static int GetCooldownGameMinutes(uint npcEntityId) =>
        GetWellLikedCooldownGameMinutes(npcEntityId);

    private static int GetWellLikedCooldownGameMinutes(uint npcEntityId) =>
        npcEntityId switch
        {
            NpcEntityIds.Harold => HaroldCooldownGameMinutes,
            NpcEntityIds.Greta => GretaCooldownGameMinutes,
            NpcEntityIds.Mira => MiraCooldownGameMinutes,
            NpcEntityIds.Elsie => ElsieCooldownGameMinutes,
            NpcEntityIds.Tom => TomCooldownGameMinutes,
            NpcEntityIds.Nora => NoraCooldownGameMinutes,
            NpcEntityIds.Elias => EliasCooldownGameMinutes,
            NpcEntityIds.Ben => BenCooldownGameMinutes,
            NpcEntityIds.Lila => LilaCooldownGameMinutes,
            NpcEntityIds.Rowan => RowanCooldownGameMinutes,
            NpcEntityIds.Marcus => MarcusCooldownGameMinutes,
            NpcEntityIds.Eleanor => EleanorCooldownGameMinutes,
            _ => 0,
        };

    public static bool ShouldSucceed(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligible(tier) || !IsSupportedNpc(npcEntityId))
            return false;

        var chance = GetSuccessChancePercent(tier);
        var roll = (playerEntityId * 127 + npcEntityId * 89 + variationSeed * 57 + (uint)(totalGameMinutes % 937)) % 100;
        return roll < chance;
    }

    public static SocialInfluenceOutcomeKind ResolveOutcome(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (tier < VillageSocialStandingTier.WellLiked)
            return ResolveRespectedOutcome(playerEntityId, npcEntityId, variationSeed);

        return npcEntityId switch
        {
            NpcEntityIds.Harold => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                61,
                HaroldInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                HaroldProjectBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.ProjectBacking),

            NpcEntityIds.Greta => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                67,
                GretaInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                GretaRecoveryOutcomeWeight,
                SocialInfluenceOutcomeKind.Recovery,
                GretaItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Mira => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                71,
                MiraInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                MiraTradePrivilegeOutcomeWeight,
                SocialInfluenceOutcomeKind.TradePrivilege,
                MiraItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Elsie => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                73,
                ElsieInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                ElsieGardenBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.GardenBacking,
                ElsieItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Tom => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                79,
                TomInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                TomLumberBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.LumberBacking,
                TomItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Nora => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                83,
                NoraInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                NoraHerbalBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.HerbalBacking,
                NoraItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Elias => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                89,
                EliasInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                EliasSmithingBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.SmithingBacking,
                EliasItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Ben => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                97,
                BenInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                BenGuardBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.GuardBacking,
                BenItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Lila => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                101,
                LilaInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                LilaYouthBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.YouthBacking,
                LilaItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Rowan => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                103,
                RowanInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                RowanStoryBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.StoryBacking,
                RowanItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Marcus => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                107,
                MarcusInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                MarcusCraftingBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.CraftingBacking,
                MarcusItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Eleanor => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                109,
                EleanorInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                EleanorLegacyBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.LegacyBacking,
                EleanorItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            _ => SocialInfluenceOutcomeKind.Info,
        };
    }

    private static SocialInfluenceOutcomeKind ResolveRespectedOutcome(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed)
    {
        return npcEntityId switch
        {
            NpcEntityIds.Harold => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                61,
                SocialInfluenceOutcomeKind.ProjectBacking),

            NpcEntityIds.Greta => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                67,
                RespectedGretaInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                RespectedGretaRecoveryOutcomeWeight,
                SocialInfluenceOutcomeKind.Recovery,
                RespectedGretaItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Mira => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                71,
                RespectedMiraInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                RespectedMiraItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Elsie => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                73,
                SocialInfluenceOutcomeKind.GardenBacking),

            NpcEntityIds.Tom => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                79,
                SocialInfluenceOutcomeKind.LumberBacking),

            NpcEntityIds.Nora => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                83,
                RespectedNoraInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                RespectedNoraHerbalBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.HerbalBacking,
                RespectedNoraRecoveryOutcomeWeight,
                SocialInfluenceOutcomeKind.Recovery,
                RespectedNoraItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Elias => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                89,
                SocialInfluenceOutcomeKind.SmithingBacking),

            NpcEntityIds.Ben => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                97,
                SocialInfluenceOutcomeKind.GuardBacking),

            NpcEntityIds.Lila => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                101,
                SocialInfluenceOutcomeKind.YouthBacking),

            NpcEntityIds.Rowan => ResolveWeightedOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                103,
                RespectedRowanInfoOutcomeWeight,
                SocialInfluenceOutcomeKind.Info,
                RespectedRowanStoryBackingOutcomeWeight,
                SocialInfluenceOutcomeKind.StoryBacking,
                RespectedRowanRecoveryOutcomeWeight,
                SocialInfluenceOutcomeKind.Recovery,
                RespectedRowanItemOutcomeWeight,
                SocialInfluenceOutcomeKind.Item),

            NpcEntityIds.Marcus => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                107,
                SocialInfluenceOutcomeKind.CraftingBacking),

            NpcEntityIds.Eleanor => ResolveRespectedBackingOutcome(
                playerEntityId,
                npcEntityId,
                variationSeed,
                109,
                SocialInfluenceOutcomeKind.LegacyBacking),

            _ => SocialInfluenceOutcomeKind.Info,
        };
    }

    private static SocialInfluenceOutcomeKind ResolveRespectedBackingOutcome(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        int seedMultiplier,
        SocialInfluenceOutcomeKind backingOutcome) =>
        ResolveWeightedOutcome(
            playerEntityId,
            npcEntityId,
            variationSeed,
            seedMultiplier,
            RespectedInfoOutcomeWeight,
            SocialInfluenceOutcomeKind.Info,
            RespectedBackingOutcomeWeight,
            backingOutcome,
            RespectedSmallBonusOutcomeWeight,
            SocialInfluenceOutcomeKind.Item);

    private static SocialInfluenceOutcomeKind ResolveWeightedOutcome(
        uint playerEntityId,
        uint npcEntityId,
        uint variationSeed,
        int seedMultiplier,
        int firstWeight,
        SocialInfluenceOutcomeKind firstOutcome,
        int secondWeight,
        SocialInfluenceOutcomeKind secondOutcome,
        int thirdWeight = 0,
        SocialInfluenceOutcomeKind thirdOutcome = SocialInfluenceOutcomeKind.Info,
        int fourthWeight = 0,
        SocialInfluenceOutcomeKind fourthOutcome = SocialInfluenceOutcomeKind.Info)
    {
        var total = firstWeight + secondWeight + thirdWeight + fourthWeight;
        var roll = (playerEntityId * 131 + npcEntityId * 97 + variationSeed * (uint)seedMultiplier) % (uint)total;
        if (roll < firstWeight)
            return firstOutcome;

        roll -= (uint)firstWeight;
        if (roll < secondWeight)
            return secondOutcome;

        roll -= (uint)secondWeight;
        if (roll < thirdWeight)
            return thirdOutcome;

        return fourthOutcome;
    }

    public static int ResolveBackingProgressBonus(int stillNeeded, int accepted, int maxBonus = WellLikedBackingProgressBonus) =>
        Math.Min(maxBonus, Math.Max(0, stillNeeded - accepted));

    public static (float MoodBonus, float SocialBonus) GetRecoveryBonus(
        uint npcEntityId,
        VillageSocialStandingTier tier)
    {
        if (tier >= VillageSocialStandingTier.WellLiked)
            return npcEntityId == NpcEntityIds.Greta
                ? (GretaMoodRecovery, GretaSocialRecovery)
                : (0f, 0f);

        return npcEntityId switch
        {
            NpcEntityIds.Greta => (RespectedGretaMoodRecovery, RespectedGretaSocialRecovery),
            NpcEntityIds.Nora => (RespectedNoraMoodRecovery, RespectedNoraSocialRecovery),
            NpcEntityIds.Rowan => (RespectedRowanMoodRecovery, RespectedRowanSocialRecovery),
            _ => (0f, 0f),
        };
    }

    public static (float MoodBonus, float SocialBonus) GetGretaRecovery(VillageSocialStandingTier tier) =>
        GetRecoveryBonus(NpcEntityIds.Greta, tier);

    public static EmotionalBondFavorGrant? TryGetItemGrant(uint npcEntityId, uint variationSeed) =>
        TryGetItemGrant(npcEntityId, variationSeed, VillageSocialStandingTier.WellLiked);

    public static EmotionalBondFavorGrant? TryGetItemGrant(
        uint npcEntityId,
        uint variationSeed,
        VillageSocialStandingTier tier)
    {
        var generous = variationSeed % 3 != 2;
        var quantity = tier >= VillageSocialStandingTier.WellLiked
            ? generous ? ItemGrantGenerousQuantity : ItemGrantBaseQuantity
            : generous ? RespectedItemGrantGenerousQuantity : RespectedItemGrantQuantity;
        var rareRoll = (variationSeed * 17 + npcEntityId * 11) % 100 < RareItemGrantChancePercent;

        return npcEntityId switch
        {
            NpcEntityIds.Greta => new EmotionalBondFavorGrant(ItemType.Apple, quantity),
            NpcEntityIds.Mira => rareRoll
                ? new EmotionalBondFavorGrant(ItemType.Tool, 1)
                : new EmotionalBondFavorGrant(ItemType.Plank, quantity),
            NpcEntityIds.Elsie => new EmotionalBondFavorGrant(ItemType.Apple, quantity),
            NpcEntityIds.Tom => variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Wood, quantity)
                : new EmotionalBondFavorGrant(ItemType.Plank, quantity),
            NpcEntityIds.Nora => new EmotionalBondFavorGrant(ItemType.Apple, quantity),
            NpcEntityIds.Elias => rareRoll || variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Tool, 1)
                : new EmotionalBondFavorGrant(ItemType.Wood, quantity),
            NpcEntityIds.Ben => rareRoll || variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Tool, 1)
                : new EmotionalBondFavorGrant(ItemType.Wood, quantity),
            NpcEntityIds.Lila => variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Apple, quantity)
                : new EmotionalBondFavorGrant(ItemType.Wood, quantity),
            NpcEntityIds.Rowan => variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Wood, quantity)
                : new EmotionalBondFavorGrant(ItemType.Apple, quantity),
            NpcEntityIds.Marcus => rareRoll || variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Tool, 1)
                : new EmotionalBondFavorGrant(ItemType.Plank, quantity),
            NpcEntityIds.Eleanor => variationSeed % 2 == 0
                ? new EmotionalBondFavorGrant(ItemType.Apple, quantity)
                : new EmotionalBondFavorGrant(ItemType.Wood, quantity),
            _ => new EmotionalBondFavorGrant(ItemType.Apple, quantity),
        };
    }

    public static string? TryGetActionableInfoCounsel(
        uint npcEntityId,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (tier < VillageSocialStandingTier.WellLiked)
            return TryGetRespectedActionableInfoCounsel(npcEntityId, variationSeed);

        string[] lines = npcEntityId switch
        {
            NpcEntityIds.Harold =>
            [
                $"[Well-liked counsel: finish well stone (15 needed) before bridge wood — your next call-on backing adds +{WellLikedBackingProgressBonus} communal progress, far above ordinary standing perks.]",
                $"[Well-liked counsel: warehouse and bridge boards track plank and stone gaps — bring bulk contributions while Harold's elder backing is active for +{WellLikedBackingProgressBonus} progress.]",
            ],
            NpcEntityIds.Greta =>
            [
                "[Well-liked counsel: help inn after communal work for the strongest mood lift — honored guests recover faster here than through ordinary favors.]",
                "[Well-liked counsel: guests ask after well-liked names by evening — rest at the inn now if mood is low; Greta's recovery through your standing is unusually strong.]",
            ],
            NpcEntityIds.Mira =>
            [
                $"[Well-liked counsel: sell apples before noon, buy planks after bridge milestones — a call-on trade favor grants ~{MiraTradePrivilegeBuyDiscountPercent:F0}% extra on {MiraTradePrivilegeTransactionCount} transactions.]",
                "[Well-liked counsel: trusted regulars move stock faster — check project boards first, then sell surplus wood when communal builds spike demand.]",
            ],
            NpcEntityIds.Elsie =>
            [
                $"[Well-liked counsel: warehouse apples close food gaps fastest — garden backing from a call-on adds +{WellLikedBackingProgressBonus} progress, double ordinary Elsie project luck.]",
                "[Well-liked counsel: south beds after rain yield best — time apple contributions before the square fills for maximum project impact.]",
            ],
            NpcEntityIds.Tom =>
            [
                $"[Well-liked counsel: well and bridge both hunger for wood (20–30 each) — lumber backing from a call-on grants +{WellLikedBackingProgressBonus} progress on your next wood drop.]",
                "[Well-liked counsel: gather at the north grove before noon, then contribute wood in stacks of five — Tom counts trusted hands differently than casual helpers.]",
            ],
            NpcEntityIds.Nora =>
            [
                $"[Well-liked counsel: herb rows and apple stores track village health — herbal backing adds +{WellLikedBackingProgressBonus} on your next apple contribution.]",
                "[Well-liked counsel: help herb garden before communal food pushes — Nora's well-liked counsel targets gaps the village board shows in stored harvest.]",
            ],
            NpcEntityIds.Elias =>
            [
                $"[Well-liked counsel: help smithy when bridge wood is low — smithing backing adds +{WellLikedBackingProgressBonus} on plank/tool contributions or +{EliasSmithingActivityWoodBonus} wood from forge help.]",
                "[Well-liked counsel: bring wood to the forge after lumber runs — Elias sets aside better stock for folk Bloomtown already trusts at the anvil.]",
            ],
            NpcEntityIds.Ben =>
            [
                $"[Well-liked counsel: help patrol when the lanes feel restless — guard backing adds +{WellLikedBackingProgressBonus} on well/bridge contributions or +{BenPatrolActivityWoodBonus} wood from patrol help.]",
                "[Well-liked counsel: walk the route before dusk — Ben notices who keeps Bloomtown's paths honest before trouble shows itself.]",
            ],
            NpcEntityIds.Lila =>
            [
                $"[Well-liked counsel: help village when the lanes feel tired — youth backing adds +{WellLikedBackingProgressBonus} on warehouse/apple contributions or +{LilaVillageActivityAppleBonus} apples from village help.]",
                "[Well-liked counsel: show up before the square fills — Lila notices who keeps Bloomtown welcoming before anyone asks.]",
            ],
            NpcEntityIds.Rowan =>
            [
                $"[Well-liked counsel: listen at the story bench when the lanes grow quiet — story backing adds +{WellLikedBackingProgressBonus} on warehouse/apple contributions or +{RowanStoryActivityWoodBonus} wood from story listening.]",
                "[Well-liked counsel: sit before the market stirs — Rowan notices who keeps Bloomtown's memory alive before anyone asks.]",
            ],
            NpcEntityIds.Marcus =>
            [
                $"[Well-liked counsel: help workshop when bridge planks run low — crafting backing adds +{WellLikedBackingProgressBonus} on plank/tool contributions or +{MarcusCraftingActivityPlankBonus} planks from workshop help.]",
                "[Well-liked counsel: bring wood to the bench after lumber runs — Marcus sets aside better stock for folk Bloomtown already trusts at the workshop.]",
            ],
            NpcEntityIds.Eleanor =>
            [
                $"[Well-liked counsel: chat on the porch when the lanes grow quiet — legacy backing adds +{WellLikedBackingProgressBonus} on warehouse/apple contributions or +{EleanorLegacyActivityAppleBonus} apples from porch chats.]",
                "[Well-liked counsel: sit before the market stirs — Eleanor notices who keeps Bloomtown's memory alive before anyone asks.]",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetRespectedActionableInfoCounsel(uint npcEntityId, uint variationSeed)
    {
        string[] lines = npcEntityId switch
        {
            NpcEntityIds.Harold =>
            [
                $"[Respected counsel: finish well stone (15 needed) before bridge wood — your call-on backing adds +{RespectedBackingProgressBonus} communal progress, weaker than Well-liked's +{WellLikedBackingProgressBonus}.]",
                "[Respected counsel: communal projects move faster with stone before wood — Harold listens to your request, though he'd speak more freely to someone Well-liked.]",
            ],
            NpcEntityIds.Greta =>
            [
                "[Respected counsel: rest at the inn when mood dips — Respected guests may receive modest recovery or a small kitchen kindness, not the full hearth rest Well-liked guests earn.]",
                "[Respected counsel: guests mention your name with respect — Greta listens warmly, though her strongest hospitality stays reserved for Well-liked regulars.]",
            ],
            NpcEntityIds.Mira =>
            [
                "[Respected counsel: sell apples before noon and watch the project boards — at Respected standing you get useful market counsel and occasional small stock, not Mira's full trade favors.]",
                "[Respected counsel: trusted folk move stock faster — Mira listens to your request, though her sharpest trade privileges await Well-liked standing.]",
            ],
            NpcEntityIds.Elsie =>
            [
                $"[Respected counsel: south beds after rain yield best — garden backing at your standing adds +{RespectedBackingProgressBonus} progress, weaker than Well-liked backing.]",
                "[Respected counsel: food contributions land best early — Elsie listens, though not as openly as she would to someone Well-liked.]",
            ],
            NpcEntityIds.Tom =>
            [
                $"[Respected counsel: the north grove yields clean timber before noon — lumber backing at your standing adds +{RespectedBackingProgressBonus} progress only.]",
                "[Respected counsel: wood work rewards steady helpers — Tom listens, though his strongest backing awaits Well-liked standing.]",
            ],
            NpcEntityIds.Nora =>
            [
                $"[Respected counsel: herb rows catch morning dew best — herbal backing at your standing adds +{RespectedBackingProgressBonus} on your next contribution.]",
                "[Respected counsel: health in Bloomtown starts in the beds — Nora listens, though not as openly as she would to someone Well-liked.]",
            ],
            NpcEntityIds.Elias =>
            [
                $"[Respected counsel: bring wood to the forge after lumber runs — smithing backing at your standing adds +{RespectedBackingProgressBonus} progress or +{RespectedActivityBonus} wood at the smithy.]",
                "[Respected counsel: the forge rewards early helpers — Elias listens, though his strongest backing awaits Well-liked standing.]",
            ],
            NpcEntityIds.Ben =>
            [
                $"[Respected counsel: walk the lanes before dusk — guard backing at your standing adds +{RespectedBackingProgressBonus} on well/bridge work or +{RespectedActivityBonus} wood on patrol.]",
                "[Respected counsel: patrol and communal work both reward steady folk — Ben listens, though not as openly as he would to someone Well-liked.]",
            ],
            NpcEntityIds.Lila =>
            [
                $"[Respected counsel: show up before the square fills — youth backing at your standing adds +{RespectedBackingProgressBonus} on warehouse/apple work or +{RespectedActivityBonus} apples on village help.]",
                "[Respected counsel: young folk notice who keeps Bloomtown welcoming — Lila listens, though her strongest backing awaits Well-liked standing.]",
            ],
            NpcEntityIds.Rowan =>
            [
                $"[Respected counsel: sit at the bench before the square fills — story backing at your standing adds +{RespectedBackingProgressBonus} on warehouse/apple work or +{RespectedActivityBonus} wood on story listening.]",
                "[Respected counsel: old tales notice who keeps Bloomtown listening — Rowan listens, though his strongest backing awaits Well-liked standing.]",
            ],
            NpcEntityIds.Marcus =>
            [
                $"[Respected counsel: bring planks to the workshop after lumber runs — crafting backing at your standing adds +{RespectedBackingProgressBonus} progress or +{RespectedActivityBonus} planks at the bench.]",
                "[Respected counsel: the workshop rewards early helpers — Marcus listens warmly, though his strongest backing awaits Well-liked standing.]",
            ],
            NpcEntityIds.Eleanor =>
            [
                $"[Respected counsel: sit on the porch before the square fills — legacy backing at your standing adds +{RespectedBackingProgressBonus} on warehouse/apple work or +{RespectedActivityBonus} apples on porch chats.]",
                "[Respected counsel: old stories notice who keeps Bloomtown listening — Eleanor listens warmly, though her strongest backing awaits Well-liked standing.]",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static bool ResolveMiraTradePrivilegeIsBuy(uint variationSeed) =>
        variationSeed % 2 == 0;

    public static string BuildUsageHint() =>
        "call on harold | ask greta for a favor | "
        + "call on mira | ask mira for a trade favor | call on elsie | ask elsie for garden support | "
        + "call on tom | ask tom for lumber support | call on nora | ask nora for herbal support | "
        + "call on elias | ask elias for smithing support | "
        + "call on ben | ask ben for guard support | "
        + "call on lila | ask lila for help | "
        + "call on rowan | ask rowan for help | "
        + "call on marcus | ask marcus for crafting support | "
        + "call on eleanor | ask eleanor for help";

    public static string FormatPlayerRequestFeedback(string npcDisplayName, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Call on — {npcDisplayName} hears you as one of Bloomtown's most trusted neighbors (full influence).]"
            : $"[Call on — {npcDisplayName} hears you as Respected — counsel, +{RespectedBackingProgressBonus} backing, "
              + $"recovery from Greta/Nora/Rowan, and small gifts. Well-liked unlocks +{WellLikedBackingProgressBonus} backing and trade favors.]";

    public static string FormatSuccessFeedback(string npcDisplayName, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Call on — {npcDisplayName} answers with full Well-liked weight.]"
            : $"[Call on — {npcDisplayName} answers warmly — your Respected standing earned a real favor today.]";

    public static string FormatDeclineFeedback(string npcDisplayName, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Call on — {npcDisplayName} is occupied, but still respects your standing. Try again later.]"
            : $"[Call on — {npcDisplayName} is on cooldown ({RespectedCooldownGameMinutes}m Respected timer). Try again later.]";

    public static string FormatCooldownHint(uint npcEntityId, VillageSocialStandingTier tier)
    {
        var name = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var cooldown = GetCooldownGameMinutes(npcEntityId, tier);
        var tierNote = tier >= VillageSocialStandingTier.WellLiked
            ? string.Empty
            : " (longer Respected cooldown)";
        return $"Calling on {name} for social influence takes time ({cooldown} game minutes between uses{tierNote}).";
    }

    public static string FormatProjectBackingFeedback(int progressBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Harold's elder backing — because you are Well-liked, your next communal building contribution counts as +{progressBonus} extra progress (stronger than ordinary standing perks).]"
            : $"[Harold's elder backing — at Respected standing your next communal contribution counts as +{progressBonus} extra progress only; Well-liked backing would count double.]";

    public static string FormatGardenBackingFeedback(int progressBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Elsie's garden backing — because you are Well-liked, your next food-or-garden contribution counts as +{progressBonus} extra progress.]"
            : $"[Elsie's garden backing — at Respected standing your next food-or-garden contribution counts as +{progressBonus} extra progress only.]";

    public static string FormatLumberBackingFeedback(int progressBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Tom's lumber backing — because you are Well-liked, your next wood contribution counts as +{progressBonus} extra progress.]"
            : $"[Tom's lumber backing — at Respected standing your next wood contribution counts as +{progressBonus} extra progress only.]";

    public static string FormatHerbalBackingFeedback(int progressBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Nora's herbal backing — because you are Well-liked, your next herb-or-plant contribution counts as +{progressBonus} extra progress.]"
            : $"[Nora's herbal backing — at Respected standing your next herb-or-plant contribution counts as +{progressBonus} extra progress only.]";

    public static string FormatSmithingBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Elias's smithing backing — because you are Well-liked, your next smithing contribution counts as +{progressBonus} extra progress, or smithy help yields +{activityBonus} wood.]"
            : $"[Elias's smithing backing — at Respected standing your next smithing contribution counts as +{progressBonus} extra progress, or smithy help yields +{activityBonus} wood only.]";

    public static string FormatSmithingActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Elias honored your Well-liked standing — smithy help yielded +{activityBonus} extra wood through his backing.]"
            : $"[Elias honored your Respected standing — smithy help yielded +{activityBonus} extra wood, weaker than Well-liked backing.]";

    public static string FormatGuardBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Ben's guard backing — because you are Well-liked, your next well-or-bridge contribution counts as +{progressBonus} extra progress, or patrol help yields +{activityBonus} wood.]"
            : $"[Ben's guard backing — at Respected standing your next well-or-bridge contribution counts as +{progressBonus} extra progress, or patrol help yields +{activityBonus} wood only.]";

    public static string FormatGuardActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Ben honored your Well-liked standing — patrol help yielded +{activityBonus} extra wood through his backing.]"
            : $"[Ben honored your Respected standing — patrol help yielded +{activityBonus} extra wood, weaker than Well-liked backing.]";

    public static string FormatYouthBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Lila's youth backing — because you are Well-liked, your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or village help yields +{activityBonus} apples.]"
            : $"[Lila's youth backing — at Respected standing your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or village help yields +{activityBonus} apples only.]";

    public static string FormatYouthActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Lila honored your Well-liked standing — village help yielded +{activityBonus} extra apples through her backing.]"
            : $"[Lila honored your Respected standing — village help yielded +{activityBonus} extra apples, weaker than Well-liked backing.]";

    public static string FormatStoryBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Rowan's story backing — because you are Well-liked, your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or story listening yields +{activityBonus} wood.]"
            : $"[Rowan's story backing — at Respected standing your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or story listening yields +{activityBonus} wood only.]";

    public static string FormatStoryActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Rowan honored your Well-liked standing — story listening yielded +{activityBonus} extra wood through his backing.]"
            : $"[Rowan honored your Respected standing — story listening yielded +{activityBonus} extra wood, weaker than Well-liked backing.]";

    public static string FormatCraftingBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Marcus's crafting backing — because you are Well-liked, your next plank-or-tool contribution counts as +{progressBonus} extra progress, or workshop help yields +{activityBonus} planks.]"
            : $"[Marcus's crafting backing — at Respected standing your next plank-or-tool contribution counts as +{progressBonus} extra progress, or workshop help yields +{activityBonus} planks only.]";

    public static string FormatCraftingActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Marcus honored your Well-liked standing — workshop help yielded +{activityBonus} extra planks through his backing.]"
            : $"[Marcus honored your Respected standing — workshop help yielded +{activityBonus} extra planks, weaker than Well-liked backing.]";

    public static string FormatLegacyBackingFeedback(int progressBonus, int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Eleanor's legacy backing — because you are Well-liked, your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or porch chats yield +{activityBonus} apples.]"
            : $"[Eleanor's legacy backing — at Respected standing your next warehouse-or-apple contribution counts as +{progressBonus} extra progress, or porch chats yield +{activityBonus} apples only.]";

    public static string FormatLegacyActivityBackingFeedback(int activityBonus, VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? $"[Eleanor honored your Well-liked standing — porch chats yielded +{activityBonus} extra apples through her backing.]"
            : $"[Eleanor honored your Respected standing — porch chats yielded +{activityBonus} extra apples, weaker than Well-liked backing.]";

    public static string FormatTradePrivilegeFeedback(bool buyPrivilege) =>
        buyPrivilege
            ? $"[Mira's trade favor — because you are Well-liked, your next {MiraTradePrivilegeTransactionCount} buys get an extra ~{MiraTradePrivilegeBuyDiscountPercent:F0}% discount each.]"
            : $"[Mira's trade favor — because you are Well-liked, your next {MiraTradePrivilegeTransactionCount} sells earn an extra ~{MiraTradePrivilegeSellBonusPercent:F0}% payout each.]";

    public static string FormatTradePrivilegeAppliedFeedback(bool buyPrivilege, int remainingUses) =>
        buyPrivilege
            ? $"[Because Mira honored your Well-liked standing, this purchase received an extra ~{MiraTradePrivilegeBuyDiscountPercent:F0}% discount{(remainingUses > 0 ? $" ({remainingUses} favor use{(remainingUses == 1 ? "" : "s")} left)" : ".")}]"
            : $"[Because Mira honored your Well-liked standing, this sale earned an extra ~{MiraTradePrivilegeSellBonusPercent:F0}% payout{(remainingUses > 0 ? $" ({remainingUses} favor use{(remainingUses == 1 ? "" : "s")} left)" : ".")}]";

    public static string FormatRecoveryFeedback(
        float moodBonus,
        float socialBonus,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked,
        uint npcEntityId = NpcEntityIds.Greta)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return string.Empty;

        var parts = new List<string>();
        if (moodBonus > 0f)
            parts.Add($"mood +{moodBonus:F0}");
        if (socialBonus > 0f)
            parts.Add($"social need -{socialBonus:F0}");

        if (tier >= VillageSocialStandingTier.WellLiked)
            return $"[Greta's hospitality steadies you through your Well-liked standing — a stronger recovery than ordinary inn rest: {string.Join(", ", parts)}.]";

        return npcEntityId switch
        {
            NpcEntityIds.Nora =>
                $"[Nora's herbal steadiness eases you through your Respected standing — a modest recovery, gentler than Well-liked hearth rest: {string.Join(", ", parts)}.]",
            NpcEntityIds.Rowan =>
                $"[Rowan's quiet counsel steadies you through your Respected standing — a modest recovery, gentler than Well-liked hearth rest: {string.Join(", ", parts)}.]",
            _ =>
                $"[Greta's hospitality steadies you through your Respected standing — a modest recovery, gentler than what Well-liked guests receive: {string.Join(", ", parts)}.]",
        };
    }

    public static string FormatItemGrantFeedback(string npcDisplayName, EmotionalBondFavorGrant grant) =>
        FormatItemGrantFeedback(npcDisplayName, grant, VillageSocialStandingTier.WellLiked);

    public static string FormatItemGrantFeedback(
        string npcDisplayName,
        EmotionalBondFavorGrant grant,
        VillageSocialStandingTier tier)
    {
        var itemLabel = grant.Quantity == 1
            ? ItemDatabase.GetDisplayName(grant.ItemType)
            : $"{grant.Quantity} {ItemDatabase.GetDisplayName(grant.ItemType)}";

        return tier >= VillageSocialStandingTier.WellLiked
            ? $"[{npcDisplayName} shares {itemLabel} — a generous Well-liked kindness, more than casual village favors offer.]"
            : $"[{npcDisplayName} shares {itemLabel} — a modest kindness for someone Respected here.]";
    }

    public static string FormatNpcAvailability(
        uint npcEntityId,
        bool ready,
        int remainingMinutes,
        VillageSocialStandingTier tier,
        string? activePrivilege = null)
    {
        var name = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        if (!string.IsNullOrWhiteSpace(activePrivilege))
            return $"  {name}: {activePrivilege}";

        var tierNote = tier >= VillageSocialStandingTier.WellLiked
            ? "full influence"
            : "Respected influence (counsel, +1 backing, recovery, small gifts)";

        return ready
            ? $"  {name}: ready — call on {name.ToLowerInvariant()} for social influence ({tierNote}, nearby)"
            : $"  {name}: resting (~{remainingMinutes}m)";
    }

    public static string? GetActivePrivilegeLabel(
        uint npcEntityId,
        bool hasHaroldBacking,
        bool hasElsieBacking,
        bool hasMiraTradePrivilege,
        bool miraTradeIsBuy,
        bool hasTomBacking,
        bool hasNoraBacking,
        bool hasEliasBacking,
        bool hasBenBacking = false,
        bool hasLilaBacking = false,
        bool hasRowanBacking = false,
        bool hasMarcusBacking = false,
        bool hasEleanorBacking = false,
        int miraTradeRemainingUses = 0,
        int haroldBackingBonus = WellLikedBackingProgressBonus,
        int elsieBackingBonus = WellLikedBackingProgressBonus,
        int tomBackingBonus = WellLikedBackingProgressBonus,
        int noraBackingBonus = WellLikedBackingProgressBonus,
        int eliasBackingBonus = WellLikedBackingProgressBonus,
        int benBackingBonus = WellLikedBackingProgressBonus,
        int lilaBackingBonus = WellLikedBackingProgressBonus,
        int rowanBackingBonus = WellLikedBackingProgressBonus,
        int marcusBackingBonus = WellLikedBackingProgressBonus,
        int eleanorBackingBonus = WellLikedBackingProgressBonus)
    {
        return npcEntityId switch
        {
            NpcEntityIds.Harold when hasHaroldBacking =>
                $"elder backing active — next communal contribution counts +{haroldBackingBonus} extra progress",
            NpcEntityIds.Elsie when hasElsieBacking =>
                $"garden backing active — next food/garden contribution counts +{elsieBackingBonus} extra progress",
            NpcEntityIds.Mira when hasMiraTradePrivilege =>
                miraTradeIsBuy
                    ? $"trade favor active — {miraTradeRemainingUses} buy(s) with extra ~{MiraTradePrivilegeBuyDiscountPercent:F0}% discount"
                    : $"trade favor active — {miraTradeRemainingUses} sell(s) with extra ~{MiraTradePrivilegeSellBonusPercent:F0}% payout",
            NpcEntityIds.Tom when hasTomBacking =>
                $"lumber backing active — next wood contribution counts +{tomBackingBonus} extra progress",
            NpcEntityIds.Nora when hasNoraBacking =>
                $"herbal backing active — next herb/plant contribution counts +{noraBackingBonus} extra progress",
            NpcEntityIds.Elias when hasEliasBacking =>
                $"smithing backing active — +{eliasBackingBonus} on smithing contributions or +{eliasBackingBonus} wood at smithy",
            NpcEntityIds.Ben when hasBenBacking =>
                $"guard backing active — +{benBackingBonus} on well/bridge contributions or +{benBackingBonus} wood on patrol",
            NpcEntityIds.Lila when hasLilaBacking =>
                $"youth backing active — +{lilaBackingBonus} on warehouse/apple contributions or +{lilaBackingBonus} apples on village help",
            NpcEntityIds.Rowan when hasRowanBacking =>
                $"story backing active — +{rowanBackingBonus} on warehouse/apple contributions or +{rowanBackingBonus} wood on story listening",
            NpcEntityIds.Marcus when hasMarcusBacking =>
                $"crafting backing active — +{marcusBackingBonus} on plank/tool contributions or +{marcusBackingBonus} planks on workshop help",
            NpcEntityIds.Eleanor when hasEleanorBacking =>
                $"legacy backing active — +{eleanorBackingBonus} on warehouse/apple contributions or +{eleanorBackingBonus} apples on porch chats",
            _ => null,
        };
    }

    public static string? FormatSocialInfluenceActionHint(VillageSocialStandingTier tier) =>
        FormatCompactSocialInfluenceHint(tier);

    public static string? FormatCompactSocialInfluenceHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"Call-on (near NPC): full influence · ~{SuccessChancePercent}% success · per-NPC cooldown ~{HaroldCooldownGameMinutes}–{EleanorCooldownGameMinutes}m.",
            VillageSocialStandingTier.Respected =>
                $"Call-on (near NPC): Respected influence (+{RespectedBackingProgressBonus} backing, counsel, recovery, small gifts) · "
                + $"~{RespectedSuccessChancePercent}% success · {RespectedCooldownGameMinutes}m cooldown · "
                + $"Well-liked unlocks +{WellLikedBackingProgressBonus} backing, trade favors, and legacy.",
            _ => null,
        };

    public static string GetTierCooldownLabel(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? "per-NPC cooldown"
            : $"{RespectedCooldownGameMinutes}m Respected cooldown";
}