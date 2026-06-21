using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Memory;

/// <summary>
/// Server entry point for player-initiated emotional bonding commands from the client console.
/// </summary>
public sealed class EmotionalBondHandler
{
    private readonly NpcEmotionalBondService _bondService;

    public EmotionalBondHandler(NpcEmotionalBondService bondService)
    {
        _bondService = bondService;
    }

    public EmotionalBondResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EmotionalBondRequest request)
    {
        var variationSeed = playerEntityId
            + (uint)request.Action
            + request.TargetNpcEntityId
            + (uint)(playerX * 10)
            + (uint)(playerZ * 10);

        if (request.Kind == EmotionalBondRequestKind.RequestStandingFavor)
        {
            return _bondService
                .PerformStandingFavorRequestAsync(
                    playerEntityId,
                    playerX,
                    playerZ,
                    request,
                    variationSeed)
                .GetAwaiter()
                .GetResult();
        }

        if (request.Kind == EmotionalBondRequestKind.RequestSocialInfluence)
        {
            return _bondService
                .PerformSocialInfluenceRequestAsync(
                    playerEntityId,
                    playerX,
                    playerZ,
                    request,
                    variationSeed)
                .GetAwaiter()
                .GetResult();
        }

        return _bondService
            .PerformBondingActionAsync(
                playerEntityId,
                playerX,
                playerZ,
                request,
                variationSeed)
            .GetAwaiter()
            .GetResult();
    }
}