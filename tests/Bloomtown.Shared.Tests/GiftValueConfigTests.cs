using Bloomtown.Shared.Gifting;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class GiftValueConfigTests
{
    [Theory]
    [InlineData(ItemType.Wood, 2)]
    [InlineData(ItemType.Plank, 3)]
    [InlineData(ItemType.Apple, 5)]
    [InlineData(ItemType.Tool, 8)]
    [InlineData(ItemType.Stone, 2)]
    public void GetBaseAffinity_ReturnsConfiguredValues(ItemType itemType, int expected)
    {
        Assert.Equal(expected, GiftValueConfig.GetBaseAffinity(itemType));
    }

    [Fact]
    public void ElsiePreferredGift_DoublesAffinityPlusBondBonus()
    {
        var gain = GiftValueConfig.CalculateAffinityGain(NpcEntityIds.Elsie, ItemType.Apple, 1);

        Assert.Equal(13, gain);
    }

    [Fact]
    public void TomPreferredGift_DoublesAffinityPlusBondBonus()
    {
        var gain = GiftValueConfig.CalculateAffinityGain(NpcEntityIds.Tom, ItemType.Tool, 1);

        Assert.Equal(19, gain);
    }

    [Fact]
    public void NonPreferredGift_UsesBaseValueOnly()
    {
        var gain = GiftValueConfig.CalculateAffinityGain(NpcEntityIds.Elsie, ItemType.Wood, 2);

        Assert.Equal(4, gain);
    }

    [Fact]
    public void NpcGiftPreference_MatchesExpectedItems()
    {
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Elsie, ItemType.Apple));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Elsie, ItemType.Plank));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Tom, ItemType.Wood));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Tom, ItemType.Tool));
        Assert.False(NpcGiftPreference.IsPreferred(NpcEntityIds.Tom, ItemType.Apple));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Harold, ItemType.Apple));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Mira, ItemType.Plank));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Greta, ItemType.Apple));
        Assert.True(NpcGiftPreference.IsPreferred(NpcEntityIds.Greta, ItemType.Plank));
    }
}