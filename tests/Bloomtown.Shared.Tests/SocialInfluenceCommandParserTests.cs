using Bloomtown.Client;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class SocialInfluenceCommandParserTests
{
    [Theory]
    [InlineData("call on harold", NpcEntityIds.Harold)]
    [InlineData("call on greta", NpcEntityIds.Greta)]
    [InlineData("call on mira", NpcEntityIds.Mira)]
    [InlineData("call on elsie", NpcEntityIds.Elsie)]
    [InlineData("call on tom", NpcEntityIds.Tom)]
    [InlineData("call on nora", NpcEntityIds.Nora)]
    [InlineData("call on elias", NpcEntityIds.Elias)]
    [InlineData("ask harold for support", NpcEntityIds.Harold)]
    [InlineData("ask greta for a favor", NpcEntityIds.Greta)]
    [InlineData("ask mira for a trade favor", NpcEntityIds.Mira)]
    [InlineData("ask elsie for garden support", NpcEntityIds.Elsie)]
    [InlineData("ask tom for lumber support", NpcEntityIds.Tom)]
    [InlineData("ask nora for herbal support", NpcEntityIds.Nora)]
    [InlineData("ask elias for smithing support", NpcEntityIds.Elias)]
    [InlineData("call on ben", NpcEntityIds.Ben)]
    [InlineData("ask ben for guard support", NpcEntityIds.Ben)]
    [InlineData("call on lila", NpcEntityIds.Lila)]
    [InlineData("ask lila for help", NpcEntityIds.Lila)]
    [InlineData("call on rowan", NpcEntityIds.Rowan)]
    [InlineData("ask rowan for help", NpcEntityIds.Rowan)]
    [InlineData("call on marcus", NpcEntityIds.Marcus)]
    [InlineData("ask marcus for crafting support", NpcEntityIds.Marcus)]
    [InlineData("call on eleanor", NpcEntityIds.Eleanor)]
    [InlineData("ask eleanor for help", NpcEntityIds.Eleanor)]
    public void TryParse_ParsesSocialInfluenceCommands(string command, uint expectedNpcId)
    {
        Assert.True(SocialInfluenceCommandParser.TryParse(command, out var request, out var error), error);
        Assert.Equal(EmotionalBondRequestKind.RequestSocialInfluence, request.Kind);
        Assert.Equal(EmotionalBondActionKind.None, request.Action);
        Assert.Equal(expectedNpcId, request.TargetNpcEntityId);
    }

    [Theory]
    [InlineData("ask mira for help")]
    [InlineData("request favor from greta")]
    public void TryParse_RejectsInvalidCommands(string command)
    {
        Assert.False(SocialInfluenceCommandParser.TryParse(command, out _, out _));
    }
}