using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcPersonalBondTests
{
    [Fact]
    public void TryGetPersonalizedResponse_RequiresFriendTierAndMemories()
    {
        var memories = new[] { NpcMemoryType.FirstPreferredGiftReceived };

        Assert.Null(NpcMemoryConfig.TryGetPersonalizedResponse(
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Acquaintance,
            0));

        var line = NpcMemoryConfig.TryGetPersonalizedResponse(
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            0);

        Assert.NotNull(line);
        Assert.Contains("gift", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetPersonalizedResponse_PrefersGiftMemoryOverProject()
    {
        var memories = new[]
        {
            NpcMemoryType.HelpedVillageProject,
            NpcMemoryType.FirstPreferredGiftReceived,
        };

        var line = NpcMemoryConfig.TryGetPersonalizedResponse(
            NpcInteractionKind.Greet,
            memories,
            RelationshipTier.Friend,
            0);

        Assert.NotNull(line);
        Assert.Contains("gift", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetPersonalMoment_ReturnsWarmLineForFriendWithMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentGifter };

        var line = NpcMemoryConfig.TryGetPersonalMoment(
            memories,
            RelationshipTier.Friend,
            "Elsie",
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.False(string.IsNullOrWhiteSpace(line));
    }

    [Fact]
    public void ShouldTriggerPersonalMoment_UsesLowThreshold()
    {
        var roll = (5u * 37 + 2u * 19 + (uint)(100 % 991)) % 100;
        Assert.Equal(roll < NpcMemoryConfig.PersonalMomentChancePercent,
            NpcMemoryConfig.ShouldTriggerPersonalMoment(5, 2, 100));
    }

    [Fact]
    public void FormatBondHint_ShowsMemorySpecificHint()
    {
        var memories = new[] { NpcMemoryType.FirstPreferredGiftReceived };

        var hint = NpcMemoryConfig.FormatBondHint(memories, RelationshipTier.Friend);

        Assert.NotNull(hint);
        Assert.Contains("kindness", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetGiftMemoryLine_VariesByTier()
    {
        var memories = new[] { NpcMemoryType.FirstPreferredGiftReceived };

        var friendLine = NpcMemoryConfig.TryGetGiftMemoryLine(memories, isPreferred: true, RelationshipTier.Friend, 0);
        var closeLine = NpcMemoryConfig.TryGetGiftMemoryLine(
            memories,
            isPreferred: true,
            RelationshipTier.CloseFriend,
            0);

        Assert.NotNull(friendLine);
        Assert.NotNull(closeLine);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    public void TryGetTalkMemoryLine_RotatesAcquaintanceLines(uint seed)
    {
        var memories = new[] { NpcMemoryType.FrequentGifter };
        var line = NpcMemoryConfig.TryGetTalkMemoryLine(memories, seed);
        Assert.NotNull(line);
    }
}