using Bloomtown.Client;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class EmotionalBondCommandParserTests
{
    [Theory]
    [InlineData("check on elsie", EmotionalBondActionKind.CheckOn, NpcEntityIds.Elsie)]
    [InlineData("share moment with harold", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Harold)]
    [InlineData("help elsie", EmotionalBondActionKind.HelpWith, NpcEntityIds.Elsie)]
    [InlineData("check on mira", EmotionalBondActionKind.CheckOn, NpcEntityIds.Mira)]
    [InlineData("share moment with mira", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Mira)]
    [InlineData("check on tom", EmotionalBondActionKind.CheckOn, NpcEntityIds.Tom)]
    [InlineData("share moment with tom", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Tom)]
    [InlineData("help tom", EmotionalBondActionKind.HelpWith, NpcEntityIds.Tom)]
    [InlineData("spend time with elsie", EmotionalBondActionKind.SpendTime, NpcEntityIds.Elsie)]
    [InlineData("sit with tom", EmotionalBondActionKind.SpendTime, NpcEntityIds.Tom)]
    [InlineData("check on greta", EmotionalBondActionKind.CheckOn, NpcEntityIds.Greta)]
    [InlineData("share moment with greta", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Greta)]
    [InlineData("help greta", EmotionalBondActionKind.HelpWith, NpcEntityIds.Greta)]
    [InlineData("spend time with greta", EmotionalBondActionKind.SpendTime, NpcEntityIds.Greta)]
    [InlineData("check on nora", EmotionalBondActionKind.CheckOn, NpcEntityIds.Nora)]
    [InlineData("share moment with nora", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Nora)]
    [InlineData("help nora", EmotionalBondActionKind.HelpWith, NpcEntityIds.Nora)]
    [InlineData("spend time with nora", EmotionalBondActionKind.SpendTime, NpcEntityIds.Nora)]
    [InlineData("check on elias", EmotionalBondActionKind.CheckOn, NpcEntityIds.Elias)]
    [InlineData("share moment with elias", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Elias)]
    [InlineData("help elias", EmotionalBondActionKind.HelpWith, NpcEntityIds.Elias)]
    [InlineData("spend time with elias", EmotionalBondActionKind.SpendTime, NpcEntityIds.Elias)]
    [InlineData("check on ben", EmotionalBondActionKind.CheckOn, NpcEntityIds.Ben)]
    [InlineData("share moment with ben", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Ben)]
    [InlineData("help ben", EmotionalBondActionKind.HelpWith, NpcEntityIds.Ben)]
    [InlineData("spend time with ben", EmotionalBondActionKind.SpendTime, NpcEntityIds.Ben)]
    [InlineData("check on lila", EmotionalBondActionKind.CheckOn, NpcEntityIds.Lila)]
    [InlineData("share moment with lila", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Lila)]
    [InlineData("help lila", EmotionalBondActionKind.HelpWith, NpcEntityIds.Lila)]
    [InlineData("spend time with lila", EmotionalBondActionKind.SpendTime, NpcEntityIds.Lila)]
    [InlineData("check on rowan", EmotionalBondActionKind.CheckOn, NpcEntityIds.Rowan)]
    [InlineData("share moment with rowan", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Rowan)]
    [InlineData("help rowan", EmotionalBondActionKind.HelpWith, NpcEntityIds.Rowan)]
    [InlineData("spend time with rowan", EmotionalBondActionKind.SpendTime, NpcEntityIds.Rowan)]
    [InlineData("check on marcus", EmotionalBondActionKind.CheckOn, NpcEntityIds.Marcus)]
    [InlineData("share moment with marcus", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Marcus)]
    [InlineData("help marcus", EmotionalBondActionKind.HelpWith, NpcEntityIds.Marcus)]
    [InlineData("spend time with marcus", EmotionalBondActionKind.SpendTime, NpcEntityIds.Marcus)]
    [InlineData("check on eleanor", EmotionalBondActionKind.CheckOn, NpcEntityIds.Eleanor)]
    [InlineData("share moment with eleanor", EmotionalBondActionKind.ShareMoment, NpcEntityIds.Eleanor)]
    [InlineData("help eleanor", EmotionalBondActionKind.HelpWith, NpcEntityIds.Eleanor)]
    [InlineData("spend time with eleanor", EmotionalBondActionKind.SpendTime, NpcEntityIds.Eleanor)]
    public void TryParse_ParsesBondingCommands(string command, EmotionalBondActionKind action, uint npcId)
    {
        Assert.True(EmotionalBondCommandParser.TryParse(command, out var request, out var error), error);
        Assert.Equal(EmotionalBondRequestKind.Perform, request.Kind);
        Assert.Equal(action, request.Action);
        Assert.Equal(npcId, request.TargetNpcEntityId);
    }

    [Fact]
    public void TryParse_RejectsCommunityHelpCommands()
    {
        Assert.False(EmotionalBondCommandParser.TryParse("help garden", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help well", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help lumber", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help inn", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help smithy", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help workshop", out _, out _));
        Assert.False(EmotionalBondCommandParser.TryParse("help village", out _, out _));
    }

    [Fact]
    public void TryParse_RejectsUnknownNpc()
    {
        Assert.False(EmotionalBondCommandParser.TryParse("check on bob", out _, out var error));
        Assert.Contains("elsie", error, StringComparison.OrdinalIgnoreCase);
    }
}