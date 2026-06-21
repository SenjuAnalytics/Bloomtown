using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Warm, memory-aware dialogue pools for deeper player–NPC personal bonds.
/// </summary>
internal static class NpcPersonalBondDialogue
{
    internal static string[] GetPersonalizedLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier)
    {
        return (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.FirstPreferredGiftReceived, RelationshipTier.CloseFriend) =>
            [
                "I still think about that gift you brought me — you have a way of making people feel seen.",
                "Talking with you feels easy. That present you gave me told me plenty about who you are.",
                "You remembered what I like. I don't forget kindness like that, especially from you.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FirstPreferredGiftReceived, _) =>
            [
                "I keep remembering that thoughtful gift — it made me trust you a little more.",
                "Every time you stop to talk, I think of that present. It meant more than you know.",
            ],

            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGifter, RelationshipTier.CloseFriend) =>
            [
                "You always show up with something kind. I hope you know how much that matters to me.",
                "Honestly? You're one of the people who make Bloomtown feel like home.",
                "I can count on your little surprises. That's rare, and I don't take it lightly.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGifter, _) =>
            [
                "You have a habit of brightening my day — gifts, visits, all of it.",
                "I always smile when I see you coming. You've been awfully good to me.",
            ],

            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageProject, RelationshipTier.CloseFriend) =>
            [
                "You helped build this village into something warmer. I see that in you every time we talk.",
                "Bloomtown wouldn't feel the same without folks like you pitching in.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageProject, _) =>
            [
                "I heard what you did for the village project. That kind of heart stays with people.",
                "Thanks for helping Bloomtown grow. It makes conversations like this feel earned.",
            ],

            (NpcInteractionKind.Greet, NpcMemoryType.FirstPreferredGiftReceived, RelationshipTier.CloseFriend) =>
            [
                "There you are! I was hoping I'd see you — you always know how to make a day brighter.",
                "Ah, perfect timing. I was just thinking about that gift you gave me.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FirstPreferredGiftReceived, _) =>
            [
                "Oh, hello! Good to see you — your last gift still makes me smile.",
                "Hey — I was hoping you'd wander by today.",
            ],

            (NpcInteractionKind.Greet, NpcMemoryType.FrequentGifter, RelationshipTier.CloseFriend) =>
            [
                "You again! I swear you appear right when I need a friendly face.",
                "Hello, you — familiar footsteps, familiar warmth. Good to see you.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentGifter, _) =>
            [
                "Back again? You always brighten the neighborhood when you visit.",
                "Oh, hi! I was just thinking how often you make people here feel cared for.",
            ],

            (NpcInteractionKind.Greet, NpcMemoryType.HelpedVillageProject, _) =>
            [
                "Hello! Bloomtown's lucky to have someone like you around.",
                "Good to see you — the village still talks about what you helped build.",
            ],

            _ => [],
        };
    }

    internal static string[] GetGiftMemoryLines(NpcMemoryType memoryType, bool isPreferred, RelationshipTier tier)
    {
        return (memoryType, isPreferred, tier) switch
        {
            (NpcMemoryType.FirstPreferredGiftReceived, true, RelationshipTier.CloseFriend) =>
            [
                "You remembered what I like — that's the sort of thing I carry with me.",
                "Another perfect choice. You know me better than most people here.",
            ],
            (NpcMemoryType.FirstPreferredGiftReceived, true, _) =>
            [
                "You remembered what I like — that stays with me.",
                "Still can't believe you knew exactly what I'd love.",
            ],
            (NpcMemoryType.FrequentGifter, _, RelationshipTier.CloseFriend) =>
            [
                "Another gift? You're spoiling me — and I won't pretend I mind.",
                "You always show up with kindness. I'm lucky you're in my life here.",
            ],
            (NpcMemoryType.FrequentGifter, _, _) =>
            [
                "Another gift? You're far too kind, but I won't say no.",
                "You really don't have to, but I'm always glad when you do.",
            ],
            (NpcMemoryType.HelpedVillageProject, _, _) =>
            [
                "After everything you did for Bloomtown, this feels like friendship in a nutshell.",
                "You've already given the village so much — this is just icing.",
            ],
            _ => [],
        };
    }

    internal static string[] GetAcquaintanceMemoryLines(NpcInteractionKind kind, NpcMemoryType memoryType)
    {
        return (kind, memoryType) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.FirstPreferredGiftReceived) =>
            [
                "I still remember that thoughtful gift you brought me — it meant a lot.",
                "That present you gave me? I think of it when you visit.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGifter) =>
            [
                "You always seem to bring little surprises when you visit. I appreciate that.",
                "You're generous — I notice, even if I don't say it every time.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageProject) =>
            [
                "I heard you helped with the village project. Bloomtown doesn't forget kindness like that.",
                "Folks still mention your help with the village. Thank you.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentGifter) =>
            [
                "Back again? You always brighten the neighborhood.",
                "Oh, hello — good to see a familiar friendly face.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FirstPreferredGiftReceived) =>
            [
                "Good to see you — I was just thinking about your last gift.",
                "Hey! Your last gift still makes me smile.",
            ],
            _ => [],
        };
    }

    internal static string[] GetPersonalMomentLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        string npcName)
    {
        _ = npcName;
        return (memoryType, tier) switch
        {
            (NpcMemoryType.FirstPreferredGiftReceived, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking about that gift you gave me — you have a gift for making people feel welcome.",
                "Funny thing — I told myself I'd thank you again today. So... thank you.",
            ],
            (NpcMemoryType.FirstPreferredGiftReceived, _) =>
            [
                "Hey — I was just thinking about that gift you brought me.",
                "Still smiling about that present you gave me the other day.",
            ],
            (NpcMemoryType.FrequentGifter, RelationshipTier.CloseFriend) =>
            [
                "You know, I think I recognize your footsteps now. That's a good feeling.",
                "I feel like I know you properly now — not just as a visitor, but as someone I trust.",
            ],
            (NpcMemoryType.FrequentGifter, _) =>
            [
                "You always brighten my day when you wander past.",
                "It's nice seeing a familiar friendly face nearby — I notice when you're around.",
            ],
            (NpcMemoryType.HelpedVillageProject, _) =>
            [
                "Thanks again for helping with the village project. I don't say it enough.",
                "Bloomtown feels a little stronger because of folks like you — I mean that.",
            ],
            _ => [],
        };
    }
}