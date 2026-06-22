using System;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client.Console
{
    /// <summary>
    /// Parses greet/talk/interact commands — same grammar as console client.
    /// </summary>
    public static class InteractionCommandParser
    {
        public readonly struct ParsedCommand
        {
            public readonly NpcInteractionKind Kind;
            public readonly uint TargetNpcEntityId;

            public ParsedCommand(NpcInteractionKind kind, uint targetNpcEntityId)
            {
                Kind = kind;
                TargetNpcEntityId = targetNpcEntityId;
            }
        }

        public static bool TryParse(string commandLine, out ParsedCommand command, out string errorMessage)
        {
            command = default;
            errorMessage = string.Empty;

            var normalized = commandLine.Trim().ToLowerInvariant();
            if (normalized.StartsWith('/'))
                normalized = normalized[1..];

            if (string.IsNullOrWhiteSpace(normalized))
            {
                errorMessage = "Empty command.";
                return false;
            }

            var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var index = 0;
            var kind = NpcInteractionKind.Greet;

            switch (parts[index])
            {
                case "talk":
                    kind = NpcInteractionKind.Talk;
                    index++;
                    break;

                case "greet":
                    kind = NpcInteractionKind.Greet;
                    index++;
                    break;

                case "interact":
                    index++;
                    if (index < parts.Length && parts[index] is "talk" or "greet")
                    {
                        kind = parts[index] == "talk"
                            ? NpcInteractionKind.Talk
                            : NpcInteractionKind.Greet;
                        index++;
                    }

                    break;

                default:
                    errorMessage = BuildUsageHint();
                    return false;
            }

            uint targetNpcEntityId = 0;
            if (index < parts.Length)
            {
                var npcName = parts[index];
                if (!NpcNameLookup.TryResolveEntityId(npcName, out targetNpcEntityId))
                {
                    errorMessage = $"Unknown NPC '{npcName}'. Known NPCs: {NpcNameLookup.KnownNamesList}.";
                    return false;
                }
            }

            command = new ParsedCommand(kind, targetNpcEntityId);
            return true;
        }

        public static string BuildUsageHint()
        {
            return "Unknown interaction. Try: greet, greet elsie, talk tom, interact greet elsie";
        }
    }
}