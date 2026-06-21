using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcMiraEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentMiraCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentMiraCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Mira));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsWarmLineForMarketMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedMarketOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Mira,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("market", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksConnectorToMira()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Mira,
            LegacyArchetype.Connector,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("connector", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsMarketMemoryForMira()
    {
        var memories = new[] { NpcMemoryType.HelpedMarketOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Mira, memories);

        Assert.NotNull(hint);
        Assert.Contains("market", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForMiraWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentMiraCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Mira,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsMiraLine()
    {
        var memories = new[] { NpcMemoryType.FrequentMiraCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Mira,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Mira", line, StringComparison.OrdinalIgnoreCase);
    }
}