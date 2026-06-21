using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Npc;

/// <summary>
/// Light information tips, habit recognition, and contextual ambient lines for meaningful social flavor.
/// </summary>
internal static class SocialDynamicsDialogue
{
    /// <summary>
    /// Small useful tip from Elsie, Mira, Harold, or Greta — location hints and village rhythm, not quest-critical info.
    /// </summary>
    internal static string? TryGetLightInfoTip(
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        if (!SocialDynamicsConfig.IsInfoSharingNpc(npcEntityId))
            return null;

        return npcEntityId switch
        {
            NpcEntityIds.Elsie => PickLine(GetElsieInfoTips(timeOfDay, completedProjectIds), variationSeed),
            NpcEntityIds.Mira => PickLine(GetMiraInfoTips(timeOfDay, developmentLevel), variationSeed),
            NpcEntityIds.Harold => PickLine(GetHaroldInfoTips(timeOfDay, completedProjectIds), variationSeed),
            NpcEntityIds.Greta => PickLine(GetGretaInfoTips(timeOfDay), variationSeed),
            _ => null,
        };
    }

    /// <summary>
    /// Personal habit line — NPC shows they recognize the player's routines or community role.
    /// Selected when acquaintance+ and the player has helped often or earned a social role.
    /// </summary>
    internal static string? TryGetPersonalHabitLine(
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        uint variationSeed)
    {
        if (!SocialDynamicsConfig.IsInfoSharingNpc(npcEntityId))
            return null;

        if (!SocialDynamicsConfig.IsEligibleForPersonalHabit(tier, reputation))
            return null;

        var role = CommunityReputationConfig.GetDominantSocialRole(reputation);
        var lines = (npcEntityId, tier >= RelationshipTier.Friend, role) switch
        {
            (NpcEntityIds.Elsie, true, CommunitySocialRole.GardenHelper) =>
            [
                "You always seem to find your way to the garden — I appreciate that rhythm.",
                "I told Mira you're our garden regular. She said she already knew by the state of the beds.",
            ],
            (NpcEntityIds.Elsie, true, _) =>
            [
                "Good to see you — Bloomtown's starting to learn your habits, I think.",
                "You turn up when the village needs a hand. People notice that more than they say.",
            ],
            (NpcEntityIds.Elsie, false, _) =>
            [
                "Familiar face — you're beginning to feel like part of the routine here.",
            ],
            (NpcEntityIds.Mira, true, CommunitySocialRole.MarketHelper) =>
            [
                "Market regular, are you? I set aside the better gossip when I see you coming.",
                "Trade day's smoother when you're around — vendors mention it.",
            ],
            (NpcEntityIds.Mira, true, _) =>
            [
                "Ah — I was hoping you'd stop by. The square feels friendlier when you're in it.",
                "You have a way of showing up when folks need an extra pair of hands.",
            ],
            (NpcEntityIds.Mira, false, _) =>
            [
                "Back again? Good — regular faces keep the market honest and warm.",
            ],
            (NpcEntityIds.Harold, true, CommunitySocialRole.WellKeeper) =>
            [
                "The well's lucky to have someone who keeps showing up — the village says so quietly.",
                "I hear your name near the gathering spot now. Steady work earns quiet trust.",
            ],
            (NpcEntityIds.Harold, true, _) =>
            [
                "Good timing — I've been hearing your name in the lanes lately, and kindly.",
                "You carry yourself like someone who belongs here now. That's worth noting.",
            ],
            (NpcEntityIds.Harold, false, _) =>
            [
                "Ah — I recognize your step. Bloomtown remembers people who return.",
            ],
            (NpcEntityIds.Greta, true, _) =>
            [
                "There you are — I was just telling a traveler you're one of our regulars now.",
                "You turn up when the parlor needs warmth. I notice that more than I let on.",
            ],
            (NpcEntityIds.Greta, false, _) =>
            [
                "Back again? Good — familiar faces keep an inn honest and welcoming.",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    /// <summary>
    /// Contextual ambient comment — warmer when the player is Friend+ with the speaking NPC.
    /// References recent village work and the player's social role when available.
    /// </summary>
    internal static string? TryGetContextualAmbientComment(
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        NpcInterpersonalRelationship elsieTomRelationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        var warm = tier >= RelationshipTier.Friend;
        var role = CommunityReputationConfig.GetDominantSocialRole(reputation);
        var friendlyVillage = elsieTomRelationship == NpcInterpersonalRelationship.Friendly;

        var lines = new List<string>();

        if (npcEntityId == NpcEntityIds.Elsie)
        {
            if (warm && role == CommunitySocialRole.GardenHelper)
                lines.Add("Elsie calls over, \"Garden helper — good timing. The beds could use your usual care.\"");
            else if (warm)
                lines.Add("Elsie smiles. \"There you are — I was just telling someone you always turn up when it counts.\"");
            else if (role != CommunitySocialRole.None)
                lines.Add("Elsie nods. \"Back again — the village is getting used to your help.\"");
        }

        if (npcEntityId == NpcEntityIds.Mira)
        {
            if (warm && role == CommunitySocialRole.MarketHelper)
                lines.Add("Mira waves. \"Market helper — I saved you the quiet end of the square today.\"");
            else if (warm)
                lines.Add("Mira grins. \"Good face to see — trade day's kinder when friends show up.\"");
            else if (timeOfDay == GameTimeOfDay.Morning)
                lines.Add("Mira says, \"Morning regular? The square's just waking — perfect time to look around.\"");
        }

        if (npcEntityId == NpcEntityIds.Harold)
        {
            if (warm)
                lines.Add("Harold tips his hat. \"Good to see a familiar walker — the lanes feel steadier for it.\"");
            else if (completedProjectIds.Count > 0)
                lines.Add("Harold murmurs, \"Village work's changed the rhythm — you're part of that story now.\"");
        }

        if (npcEntityId == NpcEntityIds.Greta)
        {
            if (warm)
                lines.Add("Greta calls from the counter. \"There you are — I saved you the quiet corner by the hearth.\"");
            else if (timeOfDay == GameTimeOfDay.Morning)
                lines.Add("Greta says, \"Morning traveler? Porridge hour's best for village gossip — pull up a chair.\"");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId) && warm)
            lines.Add("Someone mentions the well's been busier since the village finished it — your help didn't go unnoticed.");

        if (developmentLevel >= VillageDevelopmentLevel.Lively && friendlyVillage && warm)
            lines.Add("Soft village talk: Bloomtown sounds fuller lately — and familiar faces make the difference.");

        if (lines.Count == 0)
        {
            lines.Add(warm
                ? "A villager greets you by habit now — small thing, but it means you're known here."
                : "A neighbor nods in passing — not close yet, but no longer a stranger.");
        }

        return PickLine(lines.ToArray(), variationSeed);
    }

    private static string[] GetElsieInfoTips(
        GameTimeOfDay timeOfDay,
        IReadOnlyCollection<byte> completedProjectIds)
    {
        var lines = new List<string>
        {
            "Mira's usually at the square through trade hours — good time to swap goods or gossip.",
            "Tom's often out past the woods by mid-afternoon if you need a hand with timber errands.",
            "Harold walks the lanes around noon — he hears everything worth knowing before supper.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add("Garden beds are kindest in the morning light — I'm often there before breakfast.");

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
            lines.Add("Folks gather at the well around dusk — you'll hear who's been helping lately.");

        return lines.ToArray();
    }

    private static string[] GetMiraInfoTips(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<string>
        {
            "Elsie tends the community garden most mornings — she knows who needs what before noon.",
            "Harold keeps a slow patrol near the center — ask him what's moving in the village.",
            "Tom's route crosses the woods edge — handy if you need lumber or a straight answer.",
        };

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add("Afternoon's when the square fills up — best hour to see who's around.");

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add("More faces in the lanes lately — market day and village chores overlap nicely now.");

        return lines.ToArray();
    }

    private static string[] GetHaroldInfoTips(
        GameTimeOfDay timeOfDay,
        IReadOnlyCollection<byte> completedProjectIds)
    {
        var lines = new List<string>
        {
            "Mira opens the square when the village wakes — you'll find trade and talk there.",
            "Elsie keeps the shared garden welcoming — worth a visit if you want to pitch in.",
            "Tom checks the outer paths most days — quiet fellow, but reliable when work needs doing.",
        };

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add("Evenings, folks compare tomorrow's chores on porches — good time to listen in.");

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
            lines.Add("Foot traffic's steadier since the bridge came back — you'll meet more neighbors that way.");

        return lines.ToArray();
    }

    private static string[] GetGretaInfoTips(GameTimeOfDay timeOfDay)
    {
        var lines = new List<string>
        {
            "Mira's square fills up after lunch — good hour to hear who's trading and who's visiting.",
            "Elsie tends the garden most mornings — folk pass through the inn talking about her beds.",
            "Harold walks the lanes around noon — travelers often mention him over tea.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add("Morning's when the parlor hears the day's news first — worth a quiet listen.");

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add("Evening travelers swap stories by the hearth — you'll learn who's new in Bloomtown.");

        return lines.ToArray();
    }

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}