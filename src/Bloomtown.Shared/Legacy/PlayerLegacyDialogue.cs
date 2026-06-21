using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Legacy;

/// <summary>
/// Rare elder recognition and village-wide legacy flavor lines.
/// </summary>
internal static class PlayerLegacyDialogue
{
    internal static string[] GetElderRecognitionLines(
        NpcInteractionKind kind,
        PlayerLegacyMarker marker)
    {
        return (kind, marker) switch
        {
            (NpcInteractionKind.Talk, PlayerLegacyMarker.ContributedToWell) =>
            [
                "The well still runs clear because of what you gave when we needed it most. Bloomtown doesn't forget that.",
                "I tell newcomers about the day you helped finish our well — it's part of who we are now.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.ContributedToWell) =>
            [
                "Ah, there you are — the one who helped bring water back to the village.",
                "Good to see you. The well wouldn't be what it is without your hands on the work.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.ContributedToBridge) =>
            [
                "You were there when we mended the bridge. Folks cross it every day and think of people like you.",
                "I still remember hauling planks with you for that bridge. The village stands on small acts like that.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.ContributedToBridge) =>
            [
                "Hello — every crossing on that bridge carries a bit of your effort.",
                "Good day. The repaired bridge is one reason this town feels connected again.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.ContributedToWarehouse) =>
            [
                "The warehouse wouldn't serve the village the way it does if you hadn't pitched in.",
                "When supplies arrive now, I think of everyone who stacked stone and timber — including you.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.ContributedToWarehouse) =>
            [
                "There you are. The warehouse still hums with the work you helped finish.",
                "Hello — our stores are steadier because you showed up when it mattered.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.ElderCandidateTitle) =>
            [
                "You've become someone the whole village looks to — not loudly, but honestly.",
                "I won't dress it up: Bloomtown trusts you. That sort of regard takes years to earn.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.ElderCandidateTitle) =>
            [
                "Ah — one of the people this village would be poorer without.",
                "Good to see you. Your name comes up when we talk about who holds Bloomtown together.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.RespectedTitle) =>
            [
                "People speak well of you here. That doesn't happen by accident.",
                "You've given this village more than sweat — you've given it steadiness.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.RespectedTitle) =>
            [
                "Hello — respected around town, and deservedly so.",
                "Good to see you. The village notices when someone keeps showing up.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.BuilderTitle) =>
            [
                "The things we've built together changed how Bloomtown feels. You were part of that.",
                "I remember your work on our projects — it left a mark on the place.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.BuilderTitle) =>
            [
                "Hello — one of our builders. The village is stronger for it.",
                "Good day. Folks still mention what you helped raise here.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.HelpedCommunityProject) =>
            [
                "You pitched in when the village needed hands. We remember who answers that call.",
                "Bloomtown keeps a quiet ledger of kindness — your name is on it.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.HelpedCommunityProject) =>
            [
                "Hello — good to see someone who helps when it counts.",
                "Ah, there you are. The village remembers your contribution.",
            ],

            (NpcInteractionKind.Talk, PlayerLegacyMarker.HelperTitle) =>
            [
                "You've been lending a hand around here. People notice, even when they don't say it.",
                "Small helps add up. You've been doing your share, and it shows.",
            ],
            (NpcInteractionKind.Greet, PlayerLegacyMarker.HelperTitle) =>
            [
                "Hello — always good to see a helpful soul in Bloomtown.",
                "Good day. Your willingness to help hasn't gone unnoticed.",
            ],

            _ => [],
        };
    }

    internal static string[] GetVillageWideAmbientLines(PlayerLegacyMarker marker)
    {
        return marker switch
        {
            PlayerLegacyMarker.ContributedToWell =>
            [
                "A villager mentions how the well was finished — and your name comes up in the story.",
                "Someone near the well recalls the day the community finally drew water together.",
            ],
            PlayerLegacyMarker.ContributedToBridge =>
            [
                "You catch a fragment of conversation about who helped mend the bridge — they mean you.",
                "Footsteps on the bridge echo; someone says the crossing wouldn't be the same without you.",
            ],
            PlayerLegacyMarker.ContributedToWarehouse =>
            [
                "Near the stores, someone quietly credits you for helping finish the warehouse.",
                "A neighbor tells another that the warehouse stands firm thanks to folks like you.",
            ],
            PlayerLegacyMarker.ElderCandidateTitle =>
            [
                "You overhear villagers speaking of you with the kind of respect usually reserved for elders.",
                "A passerby nods as if your place in Bloomtown has long been settled.",
            ],
            PlayerLegacyMarker.RespectedTitle =>
            [
                "Someone nearby mentions you with genuine warmth — the village knows who you are.",
                "A quiet remark drifts by: Bloomtown is better for having you here.",
            ],
            PlayerLegacyMarker.BuilderTitle =>
            [
                "You hear talk of the projects that changed the village — your help is part of the story.",
                "A villager points out something built and adds, almost offhand, that you helped.",
            ],
            PlayerLegacyMarker.HelpedCommunityProject =>
            [
                "The village hums with small talk, and your name surfaces once — fondly, briefly.",
                "Someone nearby recalls that you pitched in on a community project. The tone is grateful.",
            ],
            PlayerLegacyMarker.HelperTitle =>
            [
                "A neighbor smiles your way — Bloomtown seems to know you as someone who helps.",
                "Soft chatter nearby suggests people appreciate what you've done around town.",
            ],
            _ => [],
        };
    }

    internal static string[] GetElderAmbientLines(PlayerLegacyMarker marker)
    {
        return marker switch
        {
            PlayerLegacyMarker.ContributedToWell =>
            [
                "Still grateful for what you gave us at the well.",
                "The well runs clear — and I still think of your help.",
            ],
            PlayerLegacyMarker.ContributedToBridge =>
            [
                "We cross easier because you showed up when the bridge needed mending.",
                "Every crossing on that bridge carries a bit of your effort.",
            ],
            PlayerLegacyMarker.ContributedToWarehouse =>
            [
                "The warehouse remembers its builders — you among them.",
                "Our stores are steadier because you helped finish the work.",
            ],
            PlayerLegacyMarker.ElderCandidateTitle =>
            [
                "The village speaks well of you. I hope you feel that.",
                "Your name comes up when we talk about who holds Bloomtown together.",
            ],
            PlayerLegacyMarker.RespectedTitle =>
            [
                "Folks know your name here — and say it kindly.",
                "People speak well of you. That doesn't happen by accident.",
            ],
            PlayerLegacyMarker.BuilderTitle =>
            [
                "What we've built, we built together — including your work.",
                "The village is stronger for what you helped raise here.",
            ],
            PlayerLegacyMarker.HelpedCommunityProject =>
            [
                "Bloomtown remembers who helps.",
                "You pitched in when it counted. We don't forget that.",
            ],
            PlayerLegacyMarker.HelperTitle =>
            [
                "Good to see you. This village notices helpful hands.",
                "Your willingness to help hasn't gone unnoticed.",
            ],
            _ => [],
        };
    }
}