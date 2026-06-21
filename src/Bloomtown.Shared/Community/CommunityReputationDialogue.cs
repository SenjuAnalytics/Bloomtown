using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Community;

/// <summary>Recurring acknowledgment and social-role flavor lines for community participation.</summary>
internal static class CommunityReputationDialogue
{
    /// <summary>
    /// Dependence-flavored lines when the player has earned a named social role in this area.
    /// Selected before generic recurring lines so the village feels like it leans on the player.
    /// </summary>
    internal static string? TryGetRoleDependenceAcknowledgment(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint npcEntityId,
        uint variationSeed)
    {
        if (role == CommunitySocialRole.None || !CommunityReputationConfig.IsActivityAlignedWithSocialRole(role, activity))
            return null;

        if (npcEntityId == NpcEntityIds.Elsie)
            return TryGetElsieDependenceLine(activity, role, variationSeed);

        if (npcEntityId == NpcEntityIds.Tom)
            return TryGetTomDependenceLine(activity, role, variationSeed);

        return TryGetGenericDependenceLine(activity, role, variationSeed);
    }

    /// <summary>
    /// Softer lines when the player helps often but has not earned a named role yet —
    /// the village is getting used to their presence without full dependence.
    /// </summary>
    internal static string? TryGetFamiliarPresenceAcknowledgment(
        CommunityActivityKind activity,
        uint npcEntityId,
        uint variationSeed)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
            return TryGetElsieFamiliarPresenceLine(activity, variationSeed);

        if (npcEntityId == NpcEntityIds.Tom)
            return TryGetTomFamiliarPresenceLine(activity, variationSeed);

        return TryGetGenericFamiliarPresenceLine(activity, variationSeed);
    }

    internal static string? TryGetRecurringHelpAcknowledgment(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint npcEntityId,
        uint variationSeed)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
            return TryGetElsieRecurringLine(activity, role, variationSeed);

        if (npcEntityId == NpcEntityIds.Tom)
            return TryGetTomRecurringLine(activity, role, variationSeed);

        return TryGetGenericRecurringLine(activity, role, variationSeed);
    }

    internal static string? TryGetInteractionRecognition(
        CommunitySocialRole role,
        CommunityActivityKind? dominantActivity,
        uint npcEntityId,
        uint variationSeed)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
            return TryGetElsieInteractionLine(role, dominantActivity, variationSeed);

        if (npcEntityId == NpcEntityIds.Tom)
            return TryGetTomInteractionLine(role, dominantActivity, variationSeed);

        return null;
    }

    internal static string? TryGetAmbientRoleComment(
        CommunitySocialRole role,
        CommunityActivityKind? dominantActivity,
        uint variationSeed)
    {
        if (role == CommunitySocialRole.None)
            return null;

        // Warmer ambient lines when the player has a dominant social role — kept rare via service cooldowns.
        var lines = role switch
        {
            CommunitySocialRole.GardenHelper =>
            [
                "You overhear someone say the community garden has a regular helper lately — the village appreciates it.",
                "Soft village talk: folks notice the shared beds look cared for more often now.",
                "Elsie's voice carries from nearby: \"Good — our garden helper's around again. That steadiness matters.\"",
                "A neighbor murmurs that the shared beds would feel emptier without someone who keeps showing up.",
            ],
            CommunitySocialRole.MarketHelper =>
            [
                "A neighbor mentions the market square runs smoother when a familiar helper shows up.",
                "You catch gossip: someone says trade days feel friendlier with a regular pair of helping hands.",
                "Someone near the lane says the square almost expects a certain helper before the bustle really starts.",
                "Warm market talk: vendors relax a little when they spot a familiar face pitching in.",
            ],
            CommunitySocialRole.WellKeeper =>
            [
                "Village talk: the well gathering spot stays welcoming — people credit a steady helper.",
                "Someone says the village well looks tidier than usual. Small upkeep, big difference.",
                "You hear a neighbor call the well \"in good hands lately\" — meant kindly, not official.",
                "Soft chatter: folks gather easier when someone reliable tends the gathering spot.",
            ],
            CommunitySocialRole.AllRoundHelper =>
            [
                "You overhear Bloomtown described as lucky to have someone who pitches in all over.",
                "Village gossip, fond and plain: one neighbor keeps showing up wherever shared work needs doing.",
                "Someone says Bloomtown runs smoother when one person keeps answering every little call for help.",
                "Warm village talk: the town feels like it knows who to look for when shared chores pile up.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        _ = dominantActivity;
        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetElsieRecurringLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        var activityLines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "Elsie smiles. \"You really do keep showing up for the garden — Bloomtown's starting to count on you.\"",
                "Elsie wipes her hands. \"I told Tom you're becoming our garden regular. He said he already noticed.\"",
                "Elsie nods warmly. \"The beds look better every time you help. People are talking — the good kind.\"",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "Elsie grins. \"You again at the market — I like that the square has a familiar helper.\"",
                "Elsie says, \"Folks mention you when trade day goes smoothly. That's a village compliment.\"",
                "Elsie laughs softly. \"Tom claims he saw you first, but I say Bloomtown knows your habits now.\"",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "Elsie sets down a bucket. \"You keep the well welcoming — that's the sort of care neighbors remember.\"",
                "Elsie smiles. \"People gather here because someone like you tends the details.\"",
                "Elsie says, \"I hear your name near the well now. Small role, but the village feels it.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (activityLines.Length > 0)
            return activityLines[(int)(variationSeed % (uint)activityLines.Length)];

        return TryGetElsieInteractionLine(role, activity, variationSeed);
    }

    private static string? TryGetTomRecurringLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "Tom leans on a rake. \"Garden again? Elsie says you're becoming part of the routine here.\"",
                "Tom nods. \"Regular help like yours — that's how shared places start feeling like home.\"",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "Tom stacks a crate. \"Market helper, huh? Village's lucky you're making a habit of this.\"",
                "Tom grins. \"Elsie already told half the lane you're reliable on trade days.\"",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "Tom wipes the well rim. \"You keep showing up here — good. Bloomtown remembers that.\"",
                "Tom says, \"Shared spots need steady hands. Yours are getting known.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length > 0)
            return lines[(int)(variationSeed % (uint)lines.Length)];

        return TryGetTomInteractionLine(role, activity, variationSeed);
    }

    private static string? TryGetGenericRecurringLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        _ = role;
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "A neighbor smiles. \"You're becoming a regular at the garden — the village notices.\"",
                "Someone nearby says, \"Good to see familiar help around the shared beds.\"",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "A villager nods. \"The market's smoother when you lend a hand — people mention it.\"",
                "Someone calls out, \"Trade day feels better with you helping set the tone.\"",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "A passerby thanks you. \"The well stays welcoming because folks like you keep showing up.\"",
                "Someone murmurs approval — your help around the well is becoming a village habit.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetElsieInteractionLine(
        CommunitySocialRole role,
        CommunityActivityKind? dominantActivity,
        uint variationSeed)
    {
        if (role == CommunitySocialRole.None)
            return null;

        var roleLines = role switch
        {
            CommunitySocialRole.GardenHelper =>
            [
                "Oh, there you are — I was just telling someone you're our garden regular these days.",
                "Good to see you. Folks say the shared beds wouldn't look half as welcoming without your help.",
                "Honestly, we count on you around the garden now. I'm glad you stopped by.",
            ],
            CommunitySocialRole.MarketHelper =>
            [
                "Hello! The market's been smoother since you started pitching in — villagers notice.",
                "I hear your name around the square now. Trade days feel friendlier with you in the mix.",
                "The square almost expects you on busy days — I'd miss your help if you were away.",
            ],
            CommunitySocialRole.WellKeeper =>
            [
                "Ah — Bloomtown's well-keeper, as some neighbors call you. I think it suits you.",
                "The gathering spot stays bright because people like you keep tending it. Thank you.",
                "Neighbors ask if you'll be by the well today. That kind of trust means something.",
            ],
            CommunitySocialRole.AllRoundHelper =>
            [
                "You're everywhere the village needs a hand lately — I hope you know that's appreciated.",
                "Tom and I both say Bloomtown's luckier since you started showing up all over.",
                "Bloomtown leans on you more than it used to — and I mean that fondly.",
            ],
            _ => Array.Empty<string>(),
        };

        if (roleLines.Length > 0)
            return roleLines[(int)(variationSeed % (uint)roleLines.Length)];

        _ = dominantActivity;
        return null;
    }

    private static string? TryGetTomInteractionLine(
        CommunitySocialRole role,
        CommunityActivityKind? dominantActivity,
        uint variationSeed)
    {
        if (role == CommunitySocialRole.None)
            return null;

        var roleLines = role switch
        {
            CommunitySocialRole.GardenHelper =>
            [
                "Hey — garden regular, huh? Elsie talks about your help more than she admits.",
                "Good timing. The beds look better when you've been through them — village knows it.",
                "Garden crew'd notice if you skipped a week. We count on you, near enough.",
            ],
            CommunitySocialRole.MarketHelper =>
            [
                "Market helper — that's what I heard someone call you. Fair title, I'd say.",
                "Elsie says trade day runs cleaner with you around. I won't argue.",
                "Square feels off when our market helper's not around. Don't tell Elsie I said that.",
            ],
            CommunitySocialRole.WellKeeper =>
            [
                "Well keeper — not official, but the village means it kindly. Good work.",
                "Shared spots stay useful when someone keeps showing up. That's you lately.",
                "Folks expect the well to look right because you keep tending it. Good reputation.",
            ],
            CommunitySocialRole.AllRoundHelper =>
            [
                "You turn up wherever Bloomtown needs a hand. Hard not to respect that.",
                "Elsie says you're everywhere at once. I say the village's glad for it.",
                "Hard not to rely on someone who answers every little call for help.",
            ],
            _ => Array.Empty<string>(),
        };

        if (roleLines.Length > 0)
            return roleLines[(int)(variationSeed % (uint)roleLines.Length)];

        _ = dominantActivity;
        return null;
    }

    private static string? TryGetElsieDependenceLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        var lines = (activity, role) switch
        {
            (CommunityActivityKind.HelpGarden, CommunitySocialRole.GardenHelper) =>
            [
                "Elsie beams. \"Honestly? We count on you for the garden now. I don't say that lightly.\"",
                "Elsie squeezes your arm. \"Tom asked if you'd be here today — the beds feel wrong when you're not.\"",
            ],
            (CommunityActivityKind.HelpMarket, CommunitySocialRole.MarketHelper) =>
            [
                "Elsie laughs. \"Trade day almost expects you now. I'd miss your hands if you stopped coming.\"",
                "Elsie nods toward the square. \"Vendors relax when our market helper shows up — I've noticed.\"",
            ],
            (CommunityActivityKind.HelpWell, CommunitySocialRole.WellKeeper) =>
            [
                "Elsie smiles. \"The well's in good hands with you. Neighbors say so more than they admit.\"",
                "Elsie sets down her pail. \"I told someone you'd tend the rim today. They looked relieved.\"",
            ],
            (_, CommunitySocialRole.AllRoundHelper) =>
            [
                "Elsie shakes her head, fond. \"You're everywhere we need you — Bloomtown's started leaning on that.\"",
                "Elsie says quietly, \"I don't know how we managed before you kept showing up all over.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetTomDependenceLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        var lines = (activity, role) switch
        {
            (CommunityActivityKind.HelpGarden, CommunitySocialRole.GardenHelper) =>
            [
                "Tom squints at the beds. \"Garden's yours to mind, near enough. Elsie'd worry if you skipped a week.\"",
                "Tom shrugs, pleased. \"Regular like you — that's why the garden feels settled.\"",
            ],
            (CommunityActivityKind.HelpMarket, CommunitySocialRole.MarketHelper) =>
            [
                "Tom jerks his chin at the stalls. \"Square counts on you now. Don't let it go to your head.\"",
                "Tom grins. \"Elsie says trade day needs you. She's usually right about people.\"",
            ],
            (CommunityActivityKind.HelpWell, CommunitySocialRole.WellKeeper) =>
            [
                "Tom taps the well rim. \"Folks expect this to look right because you keep showing up.\"",
                "Tom nods. \"Well keeper's not official, but the village means it. Good work.\"",
            ],
            (_, CommunitySocialRole.AllRoundHelper) =>
            [
                "Tom says, \"You turn up everywhere — hard not to rely on that a little.\"",
                "Tom mutters, almost shy, \"Bloomtown's luckier since you made helping a habit.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetGenericDependenceLine(
        CommunityActivityKind activity,
        CommunitySocialRole role,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "A neighbor calls out, \"Good — the garden helper's here. We were hoping.\"",
                "Someone nearby says the shared beds feel steadier when you show up.",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "A vendor waves. \"Market helper — good timing. We count on folks like you.\"",
                "Someone murmurs that trade day runs better when you're around.",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "A passerby nods. \"Well's in good hands — glad you're here again.\"",
                "Someone says the gathering spot stays welcoming because you keep tending it.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0 && role == CommunitySocialRole.AllRoundHelper)
        {
            lines =
            [
                "A villager says Bloomtown's lucky to have someone who answers every call for help.",
                "Someone nearby mentions the village leans on you more than it used to.",
            ];
        }

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetElsieFamiliarPresenceLine(
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "Elsie waves. \"Back at the garden — good. I'm getting used to seeing you here.\"",
                "Elsie smiles. \"The beds look better when you keep dropping by. People notice.\"",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "Elsie calls over, \"Market again? The square's starting to feel like your spot.\"",
                "Elsie nods. \"Familiar help on trade days — Bloomtown appreciates the habit.\"",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "Elsie says, \"Well upkeep again? I'm glad — this place stays brighter when you're around.\"",
                "Elsie wipes her hands. \"Neighbors mention you near the well now. That's a good sign.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetTomFamiliarPresenceLine(
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "Tom nods. \"Garden again? Folks are getting used to you around the beds.\"",
                "Tom says, \"Regular help like this — that's how shared places start feeling like home.\"",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "Tom stacks a crate. \"Market regular, huh? Square feels different when you show up.\"",
                "Tom grins. \"Trade day's smoother with familiar hands. Village knows it.\"",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "Tom wipes the rim. \"Back at the well — good. People are starting to expect that.\"",
                "Tom says, \"Shared spots stay useful when someone keeps showing up. That's you lately.\"",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetGenericFamiliarPresenceLine(
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden =>
            [
                "A neighbor smiles. \"Good to see a familiar face at the garden again.\"",
                "Someone says the shared beds feel more welcoming when you keep showing up.",
            ],
            CommunityActivityKind.HelpMarket =>
            [
                "A villager nods. \"Trade day feels friendlier when you're helping out.\"",
                "Someone nearby says the square's getting used to your presence.",
            ],
            CommunityActivityKind.HelpWell =>
            [
                "A passerby thanks you. \"The well stays welcoming when folks like you keep tending it.\"",
                "Someone murmurs approval — your help here is becoming a quiet village habit.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}