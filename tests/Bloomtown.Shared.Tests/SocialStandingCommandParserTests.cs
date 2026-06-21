using Bloomtown.Client;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class SocialStandingCommandParserTests
{
    [Theory]
    [InlineData("ask elsie for help")]
    [InlineData("ask tom for advice")]
    [InlineData("request favor from greta")]
    [InlineData("request help from harold")]
    [InlineData("ask nora for advice")]
    public void TryParse_ParsesStandingFavorCommands(string command)
    {
        Assert.True(SocialStandingCommandParser.TryParse(command, out var request, out var error), error);
        Assert.Equal(EmotionalBondRequestKind.RequestStandingFavor, request.Kind);
        Assert.Equal(EmotionalBondActionKind.None, request.Action);
        Assert.NotEqual(0u, request.TargetNpcEntityId);
    }

    [Theory]
    [InlineData("ask bob for help")]
    [InlineData("request favor from stranger")]
    [InlineData("help garden")]
    [InlineData("check on elsie")]
    public void TryParse_RejectsInvalidCommands(string command)
    {
        Assert.False(SocialStandingCommandParser.TryParse(command, out _, out _));
    }
}