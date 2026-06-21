using System.Collections.Concurrent;
using Bloomtown.Client.Console;
using Bloomtown.Client.Scripts.Net;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

internal static class Program
{
    private static readonly Dictionary<uint, string> KnownNpcNames = new()
    {
        [NpcEntityIds.Elsie] = "Elsie",
        [NpcEntityIds.Tom] = "Tom",
    };

    private static readonly ConcurrentQueue<string> PendingCommands = new();

    private static void Main()
    {
        var host = "127.0.0.1";
        var client = new NetworkClient();
        var dispatcher = new CommandDispatcher(client);
        var lineReader = new ConsoleLineReader();

        client.OnConnected += accept =>
        {
            ConsoleOutput.Success(
                $"Connected to server. Entity={accept.EntityId}, spawn=({accept.SpawnX:F1}, {accept.SpawnY:F1}, {accept.SpawnZ:F1})");
        };

        client.OnGatheringResponse += response =>
        {
            switch (response.Kind)
            {
                case GatheringResponseKind.Started:
                    ConsoleOutput.Info(response.Message);
                    break;
                case GatheringResponseKind.Completed:
                    ConsoleOutput.Success(response.Message);
                    break;
                default:
                    var reason = response.FailureReason != GatheringFailureReason.None
                        ? $" ({response.FailureReason})"
                        : string.Empty;
                    ConsoleOutput.Error($"{response.Message}{reason}");
                    break;
            }
        };

        client.OnCraftingResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == CraftingFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            ConsoleOutput.Success(response.Message);
        };

        client.OnEconomyResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == EconomyFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            ConsoleOutput.Success(response.Message);
        };

        client.OnInteractionResponse += response =>
        {
            var npcLabel = FormatEntityLabel(response.NpcEntityId);

            if (!response.Success)
            {
                var reason = response.FailureReason == NpcInteractionFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            var verb = response.Kind == NpcInteractionKind.Greet ? "greeted" : "talked with";
            ConsoleOutput.Success($"You {verb} {npcLabel}.");
            ConsoleOutput.Info($"{npcLabel} says: \"{response.Message}\"");
        };

        client.OnGiftResponse += response =>
        {
            var npcLabel = FormatEntityLabel(response.NpcEntityId);

            if (!response.Success)
            {
                var reason = response.FailureReason == GiftFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            ConsoleOutput.Success(response.Message);
            if (!string.IsNullOrWhiteSpace(response.NpcDialogue))
                ConsoleOutput.Info($"{npcLabel} says: \"{response.NpcDialogue}\"");
        };

        client.OnClientQueryResponse += response =>
        {
            if (!response.Success)
            {
                ConsoleOutput.Error(response.Message);
                return;
            }

            var title = response.Kind switch
            {
                ClientQueryKind.Status => "Status",
                ClientQueryKind.Nearby => "Nearby",
                ClientQueryKind.Nodes => "Resource Nodes",
                ClientQueryKind.Rest => "Rest",
                ClientQueryKind.Sleep => "Sleep",
                _ => "Query",
            };

            ConsoleOutput.WriteBlock(title, response.Message);
        };

        client.OnChestResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == ChestFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == ChestRequestKind.View)
                ConsoleOutput.WriteBlock("Personal Chest", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnHomeResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == HomeFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == HomeRequestKind.View)
                ConsoleOutput.WriteBlock("Home Storage", response.Message);
            else if (response.Kind == HomeRequestKind.Activity)
                ConsoleOutput.WriteBlock("Home Activity", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnCommunityProjectResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == CommunityProjectFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == CommunityProjectRequestKind.List)
                ConsoleOutput.WriteBlock("Community Projects", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnMilestoneResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == MilestoneFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == MilestoneRequestKind.List)
                ConsoleOutput.WriteBlock("Village Milestones", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnMilestoneNotification += message =>
        {
            ConsoleOutput.Success(message);
        };

        client.OnVillageAreaResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == VillageAreaFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == VillageAreaRequestKind.List)
                ConsoleOutput.WriteBlock("Village Areas", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnVillageAreaNotification += message =>
        {
            ConsoleOutput.Success(message);
        };

        client.OnPersonalRoutineResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == PersonalRoutineFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == PersonalRoutineRequestKind.List)
                ConsoleOutput.WriteBlock("Personal Routines", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnCommunityActivityResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == CommunityActivityFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == CommunityActivityRequestKind.List)
                ConsoleOutput.WriteBlock("Community Help", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnDailyVillageActivityResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == DailyVillageActivityFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == DailyVillageActivityRequestKind.List)
                ConsoleOutput.WriteBlock("Daily Village Leisure", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnDailyRhythmResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == DailyRhythmFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == DailyRhythmRequestKind.List)
                ConsoleOutput.WriteBlock("Daily Rhythm", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnLegacyFocusResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == LegacyFocusFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            ConsoleOutput.Success(response.Message);
        };

        client.OnEmotionalBondResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == EmotionalBondFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            ConsoleOutput.Success(response.Message);
        };

        client.OnNpcAmbientComment += (npcEntityId, message) =>
        {
            var header = npcEntityId == 0 ? "Village life" : "Nearby NPC";
            ConsoleOutput.WriteBlock(header, message);
        };

        client.OnProjectProposalResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == ProjectProposalFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind is ProjectProposalRequestKind.ListProposals)
                ConsoleOutput.WriteBlock("Village Proposals", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnProjectProposalNotification += message =>
        {
            ConsoleOutput.Success(message);
        };

        client.OnVillagePositionResponse += response =>
        {
            if (!response.Success)
            {
                var reason = response.FailureReason == VillagePositionFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                ConsoleOutput.Error($"{response.Message}{reason}");
                return;
            }

            if (response.Kind == VillagePositionRequestKind.List)
                ConsoleOutput.WriteBlock("Village Leadership", response.Message);
            else
                ConsoleOutput.Success(response.Message);
        };

        client.OnVillagePositionNotification += message =>
        {
            ConsoleOutput.Success(message);
        };

        client.OnDisconnected += info =>
        {
            ConsoleOutput.Info($"Disconnected from server: {info.Reason}");
        };

        System.Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            client.Disconnect();
        };

        if (!client.Connect(host, NetworkConstants.ServerPort))
        {
            ConsoleOutput.Error("Failed to start local network client.");
            return;
        }

        ConsoleOutput.Info("Bloomtown started — WASD to move, Ctrl+C to exit.");
        ConsoleOutput.Info("Type 'status' for your dashboard, 'help' for all commands.");
        ConsoleOutput.Info("Day 1: greet elsie · sit bench · daily · rhythm · gather wood");
        ConsoleOutput.Info(DailyVillageActivityCommandParser.BuildUsageHint());
        ConsoleOutput.Info(DailyRhythmCommandParser.BuildUsageHint());

        StartCommandInputThread(lineReader);

        var inputInterval = TimeSpan.FromMilliseconds(1000.0 / NetworkConstants.NetSendRate);
        var nextInputSend = DateTime.UtcNow;

        var connected = false;

        while (true)
        {
            client.PollEvents();

            if (!connected && client.LocalEntityId != 0)
                connected = true;

            if (connected && !client.IsConnected)
                break;

            ProcessPendingCommands(dispatcher);

            var moveX = 0f;
            var moveY = 0f;

            // Movement uses WASD only; arrow keys are reserved for command history.
            if (System.Console.KeyAvailable)
            {
                var keyInfo = System.Console.ReadKey(intercept: true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.W:
                        moveY = 1f;
                        break;
                    case ConsoleKey.S:
                        moveY = -1f;
                        break;
                    case ConsoleKey.A:
                        moveX = -1f;
                        break;
                    case ConsoleKey.D:
                        moveX = 1f;
                        break;
                }
            }

            client.SetInput(moveX, moveY, lookYaw: 0f);

            var now = DateTime.UtcNow;
            if (now >= nextInputSend)
            {
                client.SendInput();
                nextInputSend = now + inputInterval;
            }

            Thread.Sleep(1);
        }

        client.Disconnect();
    }

    private static void StartCommandInputThread(ConsoleLineReader lineReader)
    {
        var thread = new Thread(() => ReadConsoleCommands(lineReader))
        {
            IsBackground = true,
            Name = "ClientCommandInput",
        };
        thread.Start();
    }

    private static void ReadConsoleCommands(ConsoleLineReader lineReader)
    {
        while (true)
        {
            string? line;
            try
            {
                line = lineReader.ReadLine();
            }
            catch
            {
                break;
            }

            if (line is null)
                break;

            PendingCommands.Enqueue(line);
        }
    }

    private static void ProcessPendingCommands(CommandDispatcher dispatcher)
    {
        while (PendingCommands.TryDequeue(out var commandLine))
            dispatcher.Dispatch(commandLine);
    }

    private static string FormatEntityLabel(uint entityId)
    {
        if (KnownNpcNames.TryGetValue(entityId, out var npcName))
            return $"NPC {npcName} [{entityId}]";

        if (NpcEntityIds.IsNpc(entityId))
            return $"NPC [{entityId}]";

        return $"Player [{entityId}]";
    }
}