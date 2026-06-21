using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class SocialInfluenceActionConfigTests
{
    [Theory]
    [InlineData(VillageSocialStandingTier.Stranger, false)]
    [InlineData(VillageSocialStandingTier.Known, false)]
    [InlineData(VillageSocialStandingTier.Respected, true)]
    [InlineData(VillageSocialStandingTier.WellLiked, true)]
    public void IsEligible_RequiresRespectedTier(
        VillageSocialStandingTier tier,
        bool expected)
    {
        Assert.Equal(expected, SocialInfluenceActionConfig.IsEligible(tier));
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold, true)]
    [InlineData(NpcEntityIds.Greta, true)]
    [InlineData(NpcEntityIds.Mira, true)]
    [InlineData(NpcEntityIds.Elsie, true)]
    [InlineData(NpcEntityIds.Tom, true)]
    [InlineData(NpcEntityIds.Nora, true)]
    [InlineData(NpcEntityIds.Elias, true)]
    [InlineData(NpcEntityIds.Ben, true)]
    [InlineData(NpcEntityIds.Lila, true)]
    [InlineData(NpcEntityIds.Rowan, true)]
    [InlineData(NpcEntityIds.Marcus, true)]
    [InlineData(NpcEntityIds.Eleanor, true)]
    public void IsSupportedNpc_IncludesAllFocusInfluenceNpcs(uint npcEntityId, bool expected)
    {
        Assert.Equal(expected, SocialInfluenceActionConfig.IsSupportedNpc(npcEntityId));
    }

    [Fact]
    public void SupportedNpcEntityIds_IncludesAllTwelveInfluenceNpcs()
    {
        Assert.Equal(12, SocialInfluenceActionConfig.SupportedNpcEntityIds.Length);
        Assert.Contains(NpcEntityIds.Harold, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Greta, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Mira, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Elsie, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Tom, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Nora, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Elias, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Ben, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Lila, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Rowan, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Marcus, SocialInfluenceActionConfig.SupportedNpcEntityIds);
        Assert.Contains(NpcEntityIds.Eleanor, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }

    [Fact]
    public void GetCooldownGameMinutes_UsesLongCooldowns()
    {
        foreach (var npcEntityId in SocialInfluenceActionConfig.SupportedNpcEntityIds)
        {
            Assert.InRange(
                SocialInfluenceActionConfig.GetCooldownGameMinutes(npcEntityId, VillageSocialStandingTier.WellLiked),
                120,
                180);
        }
    }

    [Fact]
    public void GetCooldownGameMinutes_RespectedLongerThanWellLiked()
    {
        foreach (var npcEntityId in SocialInfluenceActionConfig.SupportedNpcEntityIds)
        {
            var respected = SocialInfluenceActionConfig.GetCooldownGameMinutes(
                npcEntityId,
                VillageSocialStandingTier.Respected);
            var wellLiked = SocialInfluenceActionConfig.GetCooldownGameMinutes(
                npcEntityId,
                VillageSocialStandingTier.WellLiked);

            Assert.InRange(respected, 150, 154);
            Assert.True(respected > wellLiked);
        }
    }

    [Fact]
    public void ShouldSucceed_UsesTierThreshold()
    {
        var roll = (5u * 127 + NpcEntityIds.Harold * 89 + 7u * 57 + (uint)(300 % 937)) % 100;
        Assert.Equal(
            roll < SocialInfluenceActionConfig.SuccessChancePercent,
            SocialInfluenceActionConfig.ShouldSucceed(
                5,
                NpcEntityIds.Harold,
                VillageSocialStandingTier.WellLiked,
                totalGameMinutes: 300,
                variationSeed: 7));
    }

    [Fact]
    public void ResolveOutcome_HaroldReturnsInfoOrProjectBacking()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Harold,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info or SocialInfluenceOutcomeKind.ProjectBacking);
    }

    [Fact]
    public void ResolveOutcome_GretaReturnsSocialOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Greta,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.Recovery
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_Respected_AllowsLimitedBonusOutcomes()
    {
        foreach (var npcEntityId in SocialInfluenceActionConfig.SupportedNpcEntityIds)
        {
            var outcome = SocialInfluenceActionConfig.ResolveOutcome(
                5,
                npcEntityId,
                variationSeed: 0,
                VillageSocialStandingTier.Respected);

            Assert.True(
                outcome is SocialInfluenceOutcomeKind.Info
                    or SocialInfluenceOutcomeKind.ProjectBacking
                    or SocialInfluenceOutcomeKind.GardenBacking
                    or SocialInfluenceOutcomeKind.LumberBacking
                    or SocialInfluenceOutcomeKind.HerbalBacking
                    or SocialInfluenceOutcomeKind.SmithingBacking
                    or SocialInfluenceOutcomeKind.GuardBacking
                    or SocialInfluenceOutcomeKind.YouthBacking
                    or SocialInfluenceOutcomeKind.StoryBacking
                    or SocialInfluenceOutcomeKind.CraftingBacking
                    or SocialInfluenceOutcomeKind.LegacyBacking
                    or SocialInfluenceOutcomeKind.Item
                    or SocialInfluenceOutcomeKind.Recovery);
            Assert.False(outcome is SocialInfluenceOutcomeKind.TradePrivilege);
        }
    }

    [Fact]
    public void ResolveOutcome_Respected_GretaCanReturnRecoveryOrItem()
    {
        var outcomes = new HashSet<SocialInfluenceOutcomeKind>();
        for (uint seed = 0; seed < 30; seed++)
        {
            outcomes.Add(SocialInfluenceActionConfig.ResolveOutcome(
                5,
                NpcEntityIds.Greta,
                seed,
                VillageSocialStandingTier.Respected));
        }

        Assert.Contains(SocialInfluenceOutcomeKind.Info, outcomes);
        Assert.True(outcomes.Contains(SocialInfluenceOutcomeKind.Recovery)
            || outcomes.Contains(SocialInfluenceOutcomeKind.Item));
        Assert.DoesNotContain(SocialInfluenceOutcomeKind.TradePrivilege, outcomes);
    }

    [Fact]
    public void ResolveOutcome_Respected_MiraNeverReturnsTradePrivilege()
    {
        for (uint seed = 0; seed < 30; seed++)
        {
            var outcome = SocialInfluenceActionConfig.ResolveOutcome(
                5,
                NpcEntityIds.Mira,
                seed,
                VillageSocialStandingTier.Respected);

            Assert.True(outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.Item);
            Assert.NotEqual(SocialInfluenceOutcomeKind.TradePrivilege, outcome);
        }
    }

    [Fact]
    public void GetSuccessChancePercent_RespectedHigherThanBeforeWellLikedUnchanged()
    {
        Assert.InRange(
            SocialInfluenceActionConfig.GetSuccessChancePercent(VillageSocialStandingTier.Respected),
            72,
            74);
        Assert.Equal(
            SocialInfluenceActionConfig.SuccessChancePercent,
            SocialInfluenceActionConfig.GetSuccessChancePercent(VillageSocialStandingTier.WellLiked));
    }

    [Fact]
    public void TryGetItemGrant_RespectedUsesSmallerQuantities()
    {
        var respectedGrant = SocialInfluenceActionConfig.TryGetItemGrant(
            NpcEntityIds.Elsie,
            variationSeed: 0,
            VillageSocialStandingTier.Respected);
        var wellLikedGrant = SocialInfluenceActionConfig.TryGetItemGrant(
            NpcEntityIds.Elsie,
            variationSeed: 0,
            VillageSocialStandingTier.WellLiked);

        Assert.NotNull(respectedGrant);
        Assert.NotNull(wellLikedGrant);
        Assert.InRange(respectedGrant.Value.Quantity, 1, 2);
        Assert.True(wellLikedGrant.Value.Quantity >= SocialInfluenceActionConfig.ItemGrantBaseQuantity);
    }

    [Fact]
    public void GetGretaRecovery_RespectedWeakerThanWellLiked()
    {
        var respected = SocialInfluenceActionConfig.GetGretaRecovery(VillageSocialStandingTier.Respected);
        var wellLiked = SocialInfluenceActionConfig.GetGretaRecovery(VillageSocialStandingTier.WellLiked);

        Assert.True(respected.MoodBonus < wellLiked.MoodBonus);
        Assert.True(respected.SocialBonus < wellLiked.SocialBonus);
    }

    [Fact]
    public void GetRecoveryBonus_RespectedNoraAndRowanGrantModestRecovery()
    {
        var nora = SocialInfluenceActionConfig.GetRecoveryBonus(
            NpcEntityIds.Nora,
            VillageSocialStandingTier.Respected);
        var rowan = SocialInfluenceActionConfig.GetRecoveryBonus(
            NpcEntityIds.Rowan,
            VillageSocialStandingTier.Respected);
        var greta = SocialInfluenceActionConfig.GetRecoveryBonus(
            NpcEntityIds.Greta,
            VillageSocialStandingTier.Respected);

        Assert.True(nora.MoodBonus > 0f);
        Assert.True(rowan.SocialBonus > 0f);
        Assert.True(greta.MoodBonus > nora.MoodBonus);
        Assert.Equal((0f, 0f), SocialInfluenceActionConfig.GetRecoveryBonus(
            NpcEntityIds.Nora,
            VillageSocialStandingTier.WellLiked));
    }

    [Fact]
    public void ResolveOutcome_Respected_NoraAndRowanCanReturnRecovery()
    {
        var noraOutcomes = new HashSet<SocialInfluenceOutcomeKind>();
        var rowanOutcomes = new HashSet<SocialInfluenceOutcomeKind>();
        for (uint seed = 0; seed < 50; seed++)
        {
            noraOutcomes.Add(SocialInfluenceActionConfig.ResolveOutcome(
                5,
                NpcEntityIds.Nora,
                seed,
                VillageSocialStandingTier.Respected));
            rowanOutcomes.Add(SocialInfluenceActionConfig.ResolveOutcome(
                5,
                NpcEntityIds.Rowan,
                seed,
                VillageSocialStandingTier.Respected));
        }

        Assert.Contains(SocialInfluenceOutcomeKind.Recovery, noraOutcomes);
        Assert.Contains(SocialInfluenceOutcomeKind.Recovery, rowanOutcomes);
        Assert.DoesNotContain(SocialInfluenceOutcomeKind.TradePrivilege, noraOutcomes);
        Assert.DoesNotContain(SocialInfluenceOutcomeKind.TradePrivilege, rowanOutcomes);
    }

    [Fact]
    public void ResolveOutcome_MiraReturnsTradeOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Mira,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.TradePrivilege
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_ElsieReturnsGardenOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Elsie,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.GardenBacking
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_TomReturnsLumberOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Tom,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.LumberBacking
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_NoraReturnsHerbalOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Nora,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.HerbalBacking
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_EliasReturnsSmithingOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Elias,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.SmithingBacking
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void ResolveOutcome_MarcusReturnsCraftingOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            5,
            NpcEntityIds.Marcus,
            variationSeed: 0);

        Assert.True(
            outcome is SocialInfluenceOutcomeKind.Info
                or SocialInfluenceOutcomeKind.CraftingBacking
                or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void TryGetSuccessResponseLine_RespectedHaroldBackingMentionsLimitedWeight()
    {
        var line = SocialInfluenceActionDialogue.TryGetSuccessResponseLine(
            NpcEntityIds.Harold,
            SocialInfluenceOutcomeKind.ProjectBacking,
            villageNoticedMemory: false,
            variationSeed: 0,
            VillageSocialStandingTier.Respected);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Well-liked", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("+1", line, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Mira)]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Tom)]
    [InlineData(NpcEntityIds.Nora)]
    [InlineData(NpcEntityIds.Elias)]
    [InlineData(NpcEntityIds.Marcus)]
    public void TryGetSuccessResponseLine_ReturnsPersonalityLine(uint npcEntityId)
    {
        var line = SocialInfluenceActionDialogue.TryGetSuccessResponseLine(
            npcEntityId,
            SocialInfluenceOutcomeKind.Info,
            villageNoticedMemory: false,
            variationSeed: 0,
            VillageSocialStandingTier.WellLiked);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains(
            NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
            line,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetItemGrant_ReturnsStrongerRoleAppropriateItems()
    {
        var miraGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Mira, variationSeed: 0);
        var elsieGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Elsie, variationSeed: 0);
        var tomGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Tom, variationSeed: 0);
        var noraGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Nora, variationSeed: 0);
        var eliasGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Elias, variationSeed: 0);
        var gretaGrant = SocialInfluenceActionConfig.TryGetItemGrant(NpcEntityIds.Greta, variationSeed: 0);

        Assert.NotNull(miraGrant);
        Assert.NotNull(elsieGrant);
        Assert.NotNull(tomGrant);
        Assert.NotNull(noraGrant);
        Assert.NotNull(eliasGrant);
        Assert.NotNull(gretaGrant);
        Assert.True(miraGrant.Value.ItemType is ItemType.Plank or ItemType.Tool);
        Assert.Equal(ItemType.Apple, elsieGrant.Value.ItemType);
        Assert.True(tomGrant.Value.ItemType is ItemType.Wood or ItemType.Plank);
        Assert.Equal(ItemType.Apple, noraGrant.Value.ItemType);
        Assert.True(eliasGrant.Value.ItemType is ItemType.Tool or ItemType.Wood);
        Assert.InRange(elsieGrant.Value.Quantity, SocialInfluenceActionConfig.ItemGrantBaseQuantity, SocialInfluenceActionConfig.ItemGrantGenerousQuantity);
        Assert.InRange(noraGrant.Value.Quantity, SocialInfluenceActionConfig.ItemGrantBaseQuantity, SocialInfluenceActionConfig.ItemGrantGenerousQuantity);
    }

    [Fact]
    public void ResolveBackingProgressBonus_GrantsDoubleProgress()
    {
        Assert.Equal(2, SocialInfluenceActionConfig.ResolveBackingProgressBonus(stillNeeded: 10, accepted: 1));
        Assert.Equal(1, SocialInfluenceActionConfig.ResolveBackingProgressBonus(stillNeeded: 2, accepted: 1));
    }

    [Fact]
    public void GetBackingProgressBonus_RespectedGrantsOne()
    {
        Assert.Equal(1, SocialInfluenceActionConfig.GetBackingProgressBonus(VillageSocialStandingTier.Respected));
        Assert.Equal(2, SocialInfluenceActionConfig.GetBackingProgressBonus(VillageSocialStandingTier.WellLiked));
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Tom)]
    [InlineData(NpcEntityIds.Mira)]
    public void TryGetActionableInfoCounsel_ReturnsWellLikedCounsel(uint npcEntityId)
    {
        var counsel = SocialInfluenceActionConfig.TryGetActionableInfoCounsel(
            npcEntityId,
            variationSeed: 0,
            VillageSocialStandingTier.WellLiked);

        Assert.False(string.IsNullOrWhiteSpace(counsel));
        Assert.Contains("Well-liked counsel", counsel, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Ben)]
    public void TryGetActionableInfoCounsel_ReturnsRespectedCounsel(uint npcEntityId)
    {
        var counsel = SocialInfluenceActionConfig.TryGetActionableInfoCounsel(
            npcEntityId,
            variationSeed: 0,
            VillageSocialStandingTier.Respected);

        Assert.False(string.IsNullOrWhiteSpace(counsel));
        Assert.Contains("Respected counsel", counsel, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MiraTradePrivilege_UsesStrongerMultiTransactionValues()
    {
        Assert.Equal(10f, SocialInfluenceActionConfig.MiraTradePrivilegeBuyDiscountPercent);
        Assert.Equal(2, SocialInfluenceActionConfig.MiraTradePrivilegeTransactionCount);
    }

    [Fact]
    public void GretaRecovery_UsesStrongerWellLikedValues()
    {
        Assert.True(SocialInfluenceActionConfig.GretaMoodRecovery >= 12f);
        Assert.True(SocialInfluenceActionConfig.GretaSocialRecovery >= 14f);
    }

    [Fact]
    public void FormatSocialInfluenceActionHint_IncludesRespectedWithLimitedNote()
    {
        var respectedHint = SocialInfluenceActionConfig.FormatSocialInfluenceActionHint(
            VillageSocialStandingTier.Respected);
        var wellLikedHint = SocialInfluenceActionConfig.FormatSocialInfluenceActionHint(
            VillageSocialStandingTier.WellLiked);

        Assert.False(string.IsNullOrWhiteSpace(respectedHint));
        Assert.False(string.IsNullOrWhiteSpace(wellLikedHint));
        Assert.Contains("Respected influence", respectedHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("+1", respectedHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("152", respectedHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Well-liked", respectedHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Call-on", respectedHint, StringComparison.OrdinalIgnoreCase);
        Assert.Null(SocialInfluenceActionConfig.FormatSocialInfluenceActionHint(
            VillageSocialStandingTier.Known));
    }

    [Fact]
    public void BuildUsageHint_MentionsTomNoraEliasBenLilaRowanAndMarcus()
    {
        var hint = SocialInfluenceActionConfig.BuildUsageHint();

        Assert.Contains("tom", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nora", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("elias", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ben", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lila", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rowan", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("marcus", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("eleanor", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NpcEntityIds.Tom, true, false, false, false, true, false, false, false, false, false, false, 0, "lumber backing active")]
    [InlineData(NpcEntityIds.Nora, false, false, false, false, false, true, false, false, false, false, false, 0, "herbal backing active")]
    [InlineData(NpcEntityIds.Elias, false, false, false, false, false, false, true, false, false, false, false, 0, "smithing backing active")]
    [InlineData(NpcEntityIds.Ben, false, false, false, false, false, false, false, true, false, false, false, 0, "guard backing active")]
    [InlineData(NpcEntityIds.Lila, false, false, false, false, false, false, false, false, true, false, false, 0, "youth backing active")]
    [InlineData(NpcEntityIds.Rowan, false, false, false, false, false, false, false, false, false, true, false, 0, "story backing active")]
    [InlineData(NpcEntityIds.Marcus, false, false, false, false, false, false, false, false, false, false, true, 0, "crafting backing active")]
    [InlineData(NpcEntityIds.Eleanor, false, false, false, false, false, false, false, false, false, false, false, 0, "legacy backing active")]
    [InlineData(NpcEntityIds.Mira, false, false, true, true, false, false, false, false, false, false, false, 2, "trade favor active")]
    public void GetActivePrivilegeLabel_ShowsStrongerRoleBacking(
        uint npcEntityId,
        bool hasHaroldBacking,
        bool hasElsieBacking,
        bool hasMiraTradePrivilege,
        bool miraTradeIsBuy,
        bool hasTomBacking,
        bool hasNoraBacking,
        bool hasEliasBacking,
        bool hasBenBacking,
        bool hasLilaBacking,
        bool hasRowanBacking,
        bool hasMarcusBacking,
        int miraTradeRemainingUses,
        string expectedFragment)
    {
        var hasEleanorBacking = npcEntityId == NpcEntityIds.Eleanor;
        var label = SocialInfluenceActionConfig.GetActivePrivilegeLabel(
            npcEntityId,
            hasHaroldBacking,
            hasElsieBacking,
            hasMiraTradePrivilege,
            miraTradeIsBuy,
            hasTomBacking,
            hasNoraBacking,
            hasEliasBacking,
            hasBenBacking,
            hasLilaBacking,
            hasRowanBacking,
            hasMarcusBacking,
            hasEleanorBacking,
            miraTradeRemainingUses);

        Assert.NotNull(label);
        Assert.Contains(expectedFragment, label, StringComparison.OrdinalIgnoreCase);
    }
}