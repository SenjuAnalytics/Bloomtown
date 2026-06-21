using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEmotionalBondGiftTests
{
    [Theory]
    [InlineData(NpcEntityIds.Elsie, NpcMemoryType.GaveFavoriteGiftToElsie)]
    [InlineData(NpcEntityIds.Harold, NpcMemoryType.GaveFavoriteGiftToHarold)]
    [InlineData(NpcEntityIds.Mira, NpcMemoryType.GaveFavoriteGiftToMira)]
    [InlineData(NpcEntityIds.Tom, NpcMemoryType.GaveFavoriteGiftToTom)]
    [InlineData(NpcEntityIds.Greta, NpcMemoryType.GaveFavoriteGiftToGreta)]
    [InlineData(NpcEntityIds.Nora, NpcMemoryType.GaveFavoriteGiftToNora)]
    [InlineData(NpcEntityIds.Elias, NpcMemoryType.GaveFavoriteGiftToElias)]
    [InlineData(NpcEntityIds.Ben, NpcMemoryType.GaveFavoriteGiftToBen)]
    [InlineData(NpcEntityIds.Lila, NpcMemoryType.GaveFavoriteGiftToLila)]
    [InlineData(NpcEntityIds.Rowan, NpcMemoryType.GaveFavoriteGiftToRowan)]
    [InlineData(NpcEntityIds.Marcus, NpcMemoryType.GaveFavoriteGiftToMarcus)]
    [InlineData(NpcEntityIds.Eleanor, NpcMemoryType.GaveFavoriteGiftToEleanor)]
    public void GetFavoriteGiftMemoryForNpc_MapsFocusNpcs(uint npcId, NpcMemoryType expected)
    {
        Assert.Equal(expected, NpcEmotionalBondGiftConfig.GetFavoriteGiftMemoryForNpc(npcId));
    }

    [Fact]
    public void TryGetFavoriteGiftAcceptanceLine_ReturnsWarmLineForElsie()
    {
        var line = NpcEmotionalBondGiftConfig.TryGetFavoriteGiftAcceptanceLine(
            NpcEntityIds.Elsie,
            ItemType.Apple,
            RelationshipTier.Friend,
            justRecordedBondMemory: true,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Elsie", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("soften", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsGiftMemoryForTom()
    {
        var memories = new[] { NpcMemoryType.GaveFavoriteGiftToTom };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Tom, memories);

        Assert.NotNull(hint);
        Assert.Contains("gift", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsSpendTimeMemoryForMira()
    {
        var memories = new[] { NpcMemoryType.SpentQuietTimeWithMira };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Mira, memories);

        Assert.NotNull(hint);
        Assert.Contains("quiet", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReferencesGiftMemory()
    {
        var memories = new[] { NpcMemoryType.GaveFavoriteGiftToHarold };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Harold,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.None,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("gift", line, StringComparison.OrdinalIgnoreCase);
    }
}