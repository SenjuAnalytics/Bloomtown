using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcMemoryConfigTests
{
    [Fact]
    public void GetStorageNpcEntityId_UsesVillageWideIdForProjectMemory()
    {
        Assert.Equal(
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryConfig.GetStorageNpcEntityId(5, NpcMemoryType.HelpedVillageProject));
        Assert.Equal(
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryConfig.GetStorageNpcEntityId(5, NpcMemoryType.VillageNoticedYourBonds));
        Assert.Equal(5u, NpcMemoryConfig.GetStorageNpcEntityId(5, NpcMemoryType.FrequentGifter));
    }

    [Fact]
    public void TryGetTalkMemoryLine_PrefersGiftMemoryFirst()
    {
        var memories = new[]
        {
            NpcMemoryType.HelpedVillageProject,
            NpcMemoryType.FirstPreferredGiftReceived,
        };

        var line = NpcMemoryConfig.TryGetTalkMemoryLine(memories);

        Assert.NotNull(line);
        Assert.Contains("gift", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetAmbientComment_RequiresFriendTierOrHigher()
    {
        var memories = Array.Empty<NpcMemoryType>();

        Assert.Null(NpcMemoryConfig.TryGetAmbientComment(memories, RelationshipTier.Acquaintance, 1));
        Assert.NotNull(NpcMemoryConfig.TryGetAmbientComment(memories, RelationshipTier.Friend, 1));
    }

    [Fact]
    public void TryGetAmbientComment_UsesMemorySpecificLines()
    {
        var memories = new[] { NpcMemoryType.FirstPreferredGiftReceived };

        var line = NpcMemoryConfig.TryGetAmbientComment(memories, RelationshipTier.Friend, 0);

        Assert.NotNull(line);
        Assert.Contains("gift", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFirstPreferredGiftRecordedLine_IsPersonal()
    {
        var line = NpcMemoryConfig.GetFirstPreferredGiftRecordedLine();

        Assert.Contains("remember", line, StringComparison.OrdinalIgnoreCase);
    }
}