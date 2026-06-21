using Bloomtown.Client.Scripts.Net;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client.Console;

/// <summary>
/// Routes typed console commands to local handlers or server requests.
/// </summary>
internal sealed class CommandDispatcher
{
    private readonly NetworkClient _client;

    public CommandDispatcher(NetworkClient client)
    {
        _client = client;
    }

    public void Dispatch(string commandLine)
    {
        var normalized = Normalize(commandLine);
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        if (!_client.IsConnected)
        {
            ConsoleOutput.Error("Not connected to server.");
            return;
        }

        if (normalized is "help" or "commands")
        {
            ConsoleOutput.WriteBlock("Commands", CommandRegistry.BuildHelpText());
            return;
        }

        if (normalized is "status" or "info")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Status));
            return;
        }

        if (normalized is "goal" or "legacy")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Goal));
            return;
        }

        if (normalized is "nearby" or "look")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Nearby));
            return;
        }

        if (normalized == "nodes")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Nodes));
            return;
        }

        if (normalized == "rest")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Rest));
            return;
        }

        if (normalized == "sleep")
        {
            _client.SendClientQuery(new ClientQueryRequest(ClientQueryKind.Sleep));
            return;
        }

        if (HomeActivityCommandParser.TryParse(commandLine, out var activityRequest, out _))
        {
            _client.SendHomeRequest(activityRequest);
            return;
        }

        if (HomeCommandParser.TryParse(commandLine, out var homeRequest, out var homeError))
        {
            _client.SendHomeRequest(homeRequest);
            return;
        }

        if (GatheringCommandParser.TryParse(commandLine, out var gatheringRequest, out var gatheringError))
        {
            _client.SendGatheringRequest(gatheringRequest);
            return;
        }

        if (CraftingCommandParser.TryParse(commandLine, out var craftingCommand, out var craftingError))
        {
            _client.SendCraftingRequest(new CraftingRequest(craftingCommand.RecipeId, craftingCommand.Quantity));
            return;
        }

        if (EconomyCommandParser.TryParse(commandLine, out var economyCommand, out var economyError))
        {
            _client.SendEconomyRequest(new EconomyRequest(
                economyCommand.Kind,
                economyCommand.ItemType,
                economyCommand.Quantity,
                economyCommand.NpcEntityId));
            return;
        }

        if (InteractionCommandParser.TryParse(commandLine, out var interactionCommand, out var interactionError))
        {
            _client.SendInteraction(interactionCommand.Kind, interactionCommand.TargetNpcEntityId);
            return;
        }

        if (GiftCommandParser.TryParse(commandLine, out var giftRequest, out var giftError))
        {
            _client.SendGiftRequest(giftRequest);
            return;
        }

        if (ChestCommandParser.TryParse(commandLine, out var chestRequest, out var chestError))
        {
            _client.SendChestRequest(chestRequest);
            return;
        }

        if (CommunityProjectCommandParser.TryParse(commandLine, out var projectRequest, out var projectError))
        {
            _client.SendCommunityProjectRequest(projectRequest);
            return;
        }

        if (MilestoneCommandParser.TryParse(commandLine, out var milestoneRequest, out var milestoneError))
        {
            _client.SendMilestoneRequest(milestoneRequest);
            return;
        }

        if (DailyVillageActivityCommandParser.TryParse(commandLine, out var dailyRequest, out var dailyError))
        {
            _client.SendDailyVillageActivityRequest(dailyRequest);
            return;
        }

        if (VillageAreaCommandParser.TryParse(commandLine, out var areaRequest, out var areaError))
        {
            _client.SendVillageAreaRequest(areaRequest);
            return;
        }

        if (DailyRhythmCommandParser.TryParse(commandLine, out var rhythmRequest, out var rhythmError))
        {
            _client.SendDailyRhythmRequest(rhythmRequest);
            return;
        }

        if (PersonalRoutineCommandParser.TryParse(commandLine, out var routineRequest, out var routineError))
        {
            _client.SendPersonalRoutineRequest(routineRequest);
            return;
        }

        if (CommunityActivityCommandParser.TryParse(commandLine, out var communityRequest, out var communityError))
        {
            _client.SendCommunityActivityRequest(communityRequest);
            return;
        }

        if (LegacyFocusCommandParser.TryParse(commandLine, out var legacyFocusRequest, out var legacyFocusError))
        {
            _client.SendLegacyFocusRequest(legacyFocusRequest);
            return;
        }

        if (SocialInfluenceCommandParser.TryParse(commandLine, out var socialInfluenceRequest, out var socialInfluenceError))
        {
            _client.SendEmotionalBondRequest(socialInfluenceRequest);
            return;
        }

        if (SocialStandingCommandParser.TryParse(commandLine, out var socialStandingRequest, out var socialStandingError))
        {
            _client.SendEmotionalBondRequest(socialStandingRequest);
            return;
        }

        if (EmotionalBondCommandParser.TryParse(commandLine, out var emotionalBondRequest, out var emotionalBondError))
        {
            _client.SendEmotionalBondRequest(emotionalBondRequest);
            return;
        }

        if (ProjectProposalCommandParser.TryParse(commandLine, out var proposalRequest, out var proposalError))
        {
            _client.SendProjectProposalRequest(proposalRequest);
            return;
        }

        if (VillagePositionCommandParser.TryParse(commandLine, out var positionRequest, out var positionError))
        {
            _client.SendVillagePositionRequest(positionRequest);
            return;
        }

        var error = FirstNonEmpty(gatheringError, craftingError, economyError, interactionError, giftError, chestError, homeError, projectError, milestoneError, areaError, rhythmError, routineError, communityError, dailyError, legacyFocusError, socialInfluenceError, socialStandingError, emotionalBondError, proposalError, positionError)
                    ?? $"Unknown command '{normalized.Split(' ')[0]}'. Type 'help' for a full list.";

        ConsoleOutput.Error(error);
    }

    private static string Normalize(string commandLine)
    {
        var normalized = commandLine.Trim().ToLowerInvariant();
        if (normalized.StartsWith('/'))
            normalized = normalized[1..];

        return normalized;
    }

    private static string? FirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}