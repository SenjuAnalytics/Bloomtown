using Bloomtown.Client.Network;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client.Console
{
    /// <summary>
    /// Routes HUD/console commands to server requests (status, greet, …).
    /// </summary>
    public sealed class ClientCommandDispatcher
    {
        public enum DispatchResult
        {
            HandledLocally,
            SentToServer,
            Failed,
        }

        public DispatchResult TryDispatch(string commandLine, out string message)
        {
            message = string.Empty;

            var normalized = commandLine.Trim().ToLowerInvariant();
            if (normalized.StartsWith('/'))
                normalized = normalized[1..];

            if (string.IsNullOrWhiteSpace(normalized))
                return DispatchResult.Failed;

            if (NetworkClient.Instance == null || !NetworkClient.Instance.IsConnected)
            {
                message = "Not connected to server.";
                return DispatchResult.Failed;
            }

            if (normalized is "help" or "commands")
            {
                message = BuildHelpText();
                return DispatchResult.HandledLocally;
            }

            if (normalized is "status" or "info")
            {
                NetworkClient.Instance.SendClientQuery(ClientQueryKind.Status);
                message = "Requesting status…";
                return DispatchResult.SentToServer;
            }

            if (normalized is "nearby" or "look")
            {
                NetworkClient.Instance.SendClientQuery(ClientQueryKind.Nearby);
                message = "Scanning nearby…";
                return DispatchResult.SentToServer;
            }

            var firstWord = normalized.Split(' ')[0];
            if (firstWord is "greet" or "talk" or "interact")
            {
                if (InteractionCommandParser.TryParse(commandLine, out var interaction, out var interactionError))
                {
                    NetworkClient.Instance.SendInteraction(interaction.Kind, interaction.TargetNpcEntityId);
                    var verb = interaction.Kind == NpcInteractionKind.Greet ? "Greeting" : "Talking to";
                    var target = interaction.TargetNpcEntityId == 0
                        ? "nearest NPC"
                        : NpcNameLookup.GetDisplayNameOrDefault(interaction.TargetNpcEntityId);
                    message = $"{verb} {target}…";
                    return DispatchResult.SentToServer;
                }

                message = interactionError;
                return DispatchResult.Failed;
            }

            message = $"Unknown command '{firstWord}'. Press T, type 'help'.";
            return DispatchResult.Failed;
        }

        public static string BuildHelpText()
        {
            return
                "── Commands ──\n" +
                "status          Player dashboard\n" +
                "nearby          NPCs & objects nearby\n" +
                "greet [npc]     Greet NPC (e.g. greet elsie)\n" +
                "talk [npc]      Talk to NPC\n" +
                "interact        Greet nearest NPC\n" +
                "help            This list\n\n" +
                $"NPCs: {NpcNameLookup.KnownNamesList}";
        }
    }
}