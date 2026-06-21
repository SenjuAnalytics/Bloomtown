using System.Text;

namespace Bloomtown.Client.Console;

/// <summary>
/// Central registry of console commands and their help descriptions.
/// </summary>
internal static class CommandRegistry
{
    private static readonly CommandDefinition[] Commands =
    [
        new("help", ["commands"], CommandCategory.System,
            "Show all commands grouped by category."),
        new("status", ["info"], CommandCategory.Info,
            "Dashboard: needs, rhythm, nearby activities, social standing."),
        new("goal", ["legacy"], CommandCategory.Info,
            "Long-term village legacy goal and milestone progress."),
        new("focus", [], CommandCategory.Info,
            "Strengthen a legacy path. Usage: focus build | focus tend | focus connect"),
        new("nearby", ["look"], CommandCategory.Info,
            "NPCs and resource nodes within 24m."),
        new("nodes", [], CommandCategory.Info,
            "All resource nodes with distance and cooldown."),
        new("inventory", [], CommandCategory.Economy,
            "Full inventory and coin balance."),
        new("rest", [], CommandCategory.Housing,
            "Rest outdoors to recover energy."),
        new("sleep", [], CommandCategory.Housing,
            "Sleep at home for better energy recovery."),
        new("relax", [], CommandCategory.Housing,
            "Cozy home ritual — mood and fatigue. Usage: relax (at home)"),
        new("read", [], CommandCategory.Housing,
            "Read at home. Bonus with bookshelf."),
        new("sit", [], CommandCategory.Housing,
            "Sit by your table at home."),
        new("tea", [], CommandCategory.Housing,
            "Enjoy tea at home. Bonus with chair."),
        new("nap", [], CommandCategory.Housing,
            "Short nap at home. Bonus with bed."),
        new("home", [], CommandCategory.Housing,
            "Home storage. Usage: home storage"),
        new("upgrade", ["improve"], CommandCategory.Housing,
            "Upgrade home. Usage: upgrade house (at home with materials)"),
        new("place", ["furnish"], CommandCategory.Housing,
            "Place furniture. Usage: place bed | place chair"),
        new("house", [], CommandCategory.Housing,
            "Storage transfers. Usage: house deposit wood"),
        new("chest", [], CommandCategory.Housing,
            "Personal chest near (5,5)."),
        new("deposit", [], CommandCategory.Housing,
            "Store in chest. Usage: deposit wood [qty]"),
        new("withdraw", [], CommandCategory.Housing,
            "Take from chest. Usage: withdraw stone [qty]"),
        new("greet", [], CommandCategory.Interaction,
            "Greet a nearby NPC. Usage: greet [npc]"),
        new("talk", [], CommandCategory.Interaction,
            "Talk to a nearby NPC. Usage: talk elsie"),
        new("gift", ["give"], CommandCategory.Interaction,
            "Give an item. Usage: gift apple to elsie"),
        new("check", [], CommandCategory.Interaction,
            "Check on a focus NPC nearby. Usage: check on elsie"),
        new("spend", [], CommandCategory.Interaction,
            "Quiet time with focus NPC. Usage: spend time with tom"),
        new("share", [], CommandCategory.Interaction,
            "Share a quiet moment (Friend+). Usage: share moment with mira"),
        new("ask", [], CommandCategory.Interaction,
            "Ask for help (Respected+). Usage: ask elsie for help"),
        new("request", [], CommandCategory.Interaction,
            "Request a village favor (Respected+). Usage: request favor from greta"),
        new("call", [], CommandCategory.Interaction,
            "Social influence (Well-liked). Usage: call on tom"),
        new("interact", [], CommandCategory.Interaction,
            "Nearest or named NPC. Usage: interact greet elsie"),
        new("buy", [], CommandCategory.Economy,
            "Buy from NPC. Usage: buy apple from mira"),
        new("sell", [], CommandCategory.Economy,
            "Sell to NPC. Usage: sell wood to mira"),
        new("craft", [], CommandCategory.Crafting,
            "Craft items. Usage: craft plank 10 | craft tool"),
        new("gather", [], CommandCategory.Gathering,
            "Gather a resource. Usage: gather wood"),
        new("chop", [], CommandCategory.Gathering,
            "Chop a tree."),
        new("mine", [], CommandCategory.Gathering,
            "Mine stone."),
        new("daily", ["leisure"], CommandCategory.Community,
            "List daily activities: sit bench | chat locals | tend public garden | practice workshop"),
        new("bench", [], CommandCategory.Community,
            "Village green bench. Usage: sit bench"),
        new("watch", [], CommandCategory.Community,
            "Village outlook. Usage: watch village"),
        new("chat", [], CommandCategory.Community,
            "Social ritual. Usage: chat locals"),
        new("tend", [], CommandCategory.Community,
            "Public garden beds. Usage: tend public garden"),
        new("practice", [], CommandCategory.Community,
            "Workshop practice. Usage: practice workshop"),
        new("community", ["community-help"], CommandCategory.Community,
            "Community help list. Usage: help garden | help market | help well | ..."),
        new("browse", [], CommandCategory.Community,
            "Market Square visit. Usage: browse market"),
        new("stroll", [], CommandCategory.Community,
            "Riverside walk. Usage: stroll river"),
        new("reflect", [], CommandCategory.Community,
            "River reflection. Usage: reflect river"),
        new("areas", ["area"], CommandCategory.Community,
            "Unlockable village areas."),
        new("projects", ["project"], CommandCategory.Community,
            "Village projects and progress."),
        new("contribute", [], CommandCategory.Community,
            "Donate resources. Usage: contribute wood 10 to well"),
        new("propose", [], CommandCategory.Community,
            "Propose project (Builder+). Usage: propose \"Name\" wood 40"),
        new("proposals", ["proposal-list"], CommandCategory.Community,
            "Open votes and recent proposals."),
        new("vote", [], CommandCategory.Community,
            "Cast vote. Usage: vote yes on 5"),
        new("positions", ["leadership"], CommandCategory.Community,
            "Leadership roles and elections."),
        new("run", [], CommandCategory.Community,
            "Run for position. Usage: run for chief"),
        new("elect", [], CommandCategory.Community,
            "Vote in election. Usage: elect yes chief"),
        new("council", [], CommandCategory.Community,
            "Council vote. Usage: council yes 5"),
        new("chief", [], CommandCategory.Community,
            "Chief authority. Usage: chief approve 5"),
        new("rhythm", ["daily-rhythm"], CommandCategory.Personal,
            "Day summary & agency. Usage: rhythm | start calm | focused break | rest early | rhythm wind down"),
        new("start", [], CommandCategory.Personal,
            "Morning intent. Usage: start calm | start active"),
        new("settle", [], CommandCategory.Personal,
            "Wind down. Usage: rhythm wind down"),
        new("routines", ["routine"], CommandCategory.Personal,
            "Personal routines and ideal times."),
        new("stretch", [], CommandCategory.Personal,
            "Morning stretch. Usage: morning stretch"),
        new("wind", [], CommandCategory.Personal,
            "Evening wind down. Usage: evening wind down"),
        new("milestones", ["milestone"], CommandCategory.Milestone,
            "Unlocked milestones."),
        new("drink", [], CommandCategory.Milestone,
            "Village well. Usage: drink well"),
        new("cross", [], CommandCategory.Milestone,
            "Repaired bridge. Usage: cross bridge"),
        new("collect", [], CommandCategory.Milestone,
            "Warehouse stipend. Usage: collect stipend"),
    ];

    private static readonly string MovementHelp =
        "WASD — move (arrow keys = command history while typing)";

    private static readonly string GettingStartedHelp =
        "New here? status · greet elsie · sit bench · daily · rhythm · gather wood · help";

    private static readonly Dictionary<CommandCategory, string> CategoryIntros = new()
    {
        [CommandCategory.Info] = "Check yourself and the world around you.",
        [CommandCategory.Interaction] = "Meet villagers and deepen relationships.",
        [CommandCategory.Economy] = "Coins, trade, and inventory.",
        [CommandCategory.Gathering] = "Collect wood, stone, and other resources.",
        [CommandCategory.Crafting] = "Turn materials into useful goods.",
        [CommandCategory.Housing] = "Home, rest, furniture, and storage.",
        [CommandCategory.Community] = "Village life — daily rituals, help, and projects.",
        [CommandCategory.Milestone] = "Completed village landmarks.",
        [CommandCategory.Personal] = "Your daily pace, rhythm, and cozy routines.",
        [CommandCategory.System] = "Help and system commands.",
    };

    public static string BuildHelpText()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Bloomtown — type a command and press Enter.");
        builder.AppendLine();
        builder.AppendLine("── Getting Started ──");
        builder.AppendLine($"  {GettingStartedHelp}");
        builder.AppendLine();

        AppendCategory(builder, CommandCategory.Movement, [MovementHelp]);
        AppendCategory(builder, CommandCategory.Info);
        AppendCategory(builder, CommandCategory.Personal);
        AppendCategory(builder, CommandCategory.Interaction);
        AppendCategory(builder, CommandCategory.Community);
        AppendCategory(builder, CommandCategory.Housing);
        AppendCategory(builder, CommandCategory.Gathering);
        AppendCategory(builder, CommandCategory.Economy);
        AppendCategory(builder, CommandCategory.Crafting);
        AppendCategory(builder, CommandCategory.Milestone);
        AppendCategory(builder, CommandCategory.System);

        builder.AppendLine("Tip: Up/Down browses command history. 'status' is your best daily dashboard.");
        return builder.ToString().TrimEnd();
    }

    private static void AppendCategory(StringBuilder builder, CommandCategory category, IEnumerable<string>? extraLines = null)
    {
        builder.AppendLine($"[{category}]");
        if (CategoryIntros.TryGetValue(category, out var intro))
            builder.AppendLine($"  {intro}");

        if (extraLines is not null)
        {
            foreach (var line in extraLines)
                builder.AppendLine($"  {line}");
        }

        foreach (var command in Commands.Where(c => c.Category == category))
        {
            var names = string.Join(" / ", command.AllNames);
            builder.AppendLine($"  {names,-22} — {command.Description}");
        }

        builder.AppendLine();
    }
}