using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Legacy dialogue acknowledging the player's long social journey — Well-liked only.
/// </summary>
public static class SocialLegacyDialogue
{
    public static string? TryGetLegacyJourneyLine(
        uint npcEntityId,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, villageNoticedMemory) switch
        {
            (NpcEntityIds.Harold, true) =>
            [
                "Harold murmurs with elder's pride. \"You've become part of this village's story now — not a visitor passing through, but someone Bloomtown will remember.\"",
                "Harold tips his hat slowly. \"I watched you earn every friend and every kind word. That kind of standing doesn't fade when the seasons turn.\"",
                "Harold says quietly. \"The well hears your name each morning now. That's legacy, friend — the village carrying you forward.\"",
                "Harold leans on the well rim. \"Elders like me measure a life by who still greets you after hard winters — Bloomtown greets you warmly, every day.\"",
                "Harold adds with a rare smile. \"Folk ask me about you at first light. I tell them the truth: you belong here as much as the stones do.\"",
            ],
            (NpcEntityIds.Harold, false) =>
            [
                "Harold nods with quiet respect. \"You've become part of this village's story now — folk will speak of you long after today's work is done.\"",
                "Harold sets a hand on your shoulder. \"Trusted neighbors like you shape what Bloomtown becomes. I'm glad I saw it happen.\"",
                "Harold murmurs by the well. \"I remember when you were new to these lanes. Now half the village counts on your kindness — that's no small thing.\"",
                "Harold tips his hat. \"Your name travels farther than you know, friend. Even quiet folk speak it with respect.\"",
            ],

            (NpcEntityIds.Greta, true) =>
            [
                "Greta beams with real warmth. \"You've become part of this village's story now, love — guests at my table still ask after you by name.\"",
                "Greta fusses over your cup. \"Bloomtown remembers who showed up, who stayed, who cared. You've left a mark here — a good one.\"",
                "Greta leans in softly. \"My parlor's heard your story grow season by season. That's legacy — and you've earned every word of it.\"",
                "Greta saves your seat without asking. \"Travelers hear your name before they reach my door — always spoken with fondness. That's rare, love.\"",
                "Greta squeezes your hand. \"I keep a corner warm for folk the village is proud of. You've earned that seat a hundred times over.\"",
            ],
            (NpcEntityIds.Greta, false) =>
            [
                "Greta smiles with fondness. \"You've become part of this village's story now — the inn keeps your place warm because Bloomtown expects you.\"",
                "Greta squeezes your arm. \"Folk speak your name with pride long after you leave my table. That's the kind of neighbor villages remember.\"",
                "Greta fusses over your cup. \"Regulars like you change how an inn feels — warmer, steadier, more like home. Bloomtown notices.\"",
                "Greta whispers warmly. \"Between us — half my guests already know your name. And they always say it kindly.\"",
            ],

            (NpcEntityIds.Elsie, true) =>
            [
                "Elsie smiles with quiet pride. \"You've become part of this village's story now — the garden knew your kindness before the lanes named it.\"",
                "Elsie says gently. \"Bloomtown remembers who tended the beds, who brought apples, who kept showing up. Your legacy grows here every season.\"",
                "Elsie leans in warmly. \"Neighbors speak of you like family now. That's not a title — it's years of trust woven into village life.\"",
                "Elsie hands you a ripe apple. \"The south beds remember your footsteps. So does everyone who walks past the gate.\"",
            ],
            (NpcEntityIds.Elsie, false) =>
            [
                "Elsie nods with fondness. \"You've become part of this village's story now — folk greet you like someone the beds have known a long while.\"",
                "Elsie adds softly. \"The village carries your name gently now. That's legacy — earned one kindness at a time.\"",
                "Elsie smiles. \"I knew you before Bloomtown did. I'm glad the rest of the village caught up.\"",
            ],

            (NpcEntityIds.Rowan, true) =>
            [
                "Rowan pauses mid-tale with quiet pride. \"You've become part of Bloomtown's story now — and stories like yours are the ones we tell on cold evenings.\"",
                "Rowan sets down his woodcarving. \"I watched you earn every friend and every kind word. Villages remember neighbors like that long after the teller is gone.\"",
                "Rowan says softly. \"The story bench hears your name each evening now. That's legacy — a life woven into the tales we pass down.\"",
                "Rowan smiles faintly. \"Old stories teach us who endures. You've endured here — with grace, with patience, with real friendship.\"",
                "Rowan murmurs. \"Someday someone will sit on this bench and hear your name spoken with the same warmth I feel saying it now.\"",
            ],
            (NpcEntityIds.Rowan, false) =>
            [
                "Rowan nods with quiet respect. \"You've become part of this village's story now — folk will speak of you long after today's work is done.\"",
                "Rowan gestures to the bench. \"Trusted neighbors like you shape what Bloomtown becomes. I'm glad I witnessed your chapter.\"",
                "Rowan says warmly. \"The inn corner still holds your stories — and the village listens when your name comes up.\"",
                "Rowan adds softly. \"Not every visitor becomes part of the lore. You did — and earned it honestly.\"",
            ],

            (NpcEntityIds.Marcus, true) =>
            [
                "Marcus sets down his plane with quiet pride. \"You've become part of this village's story now — good timber and good neighbors both last generations.\"",
                "Marcus wipes his hands. \"I watched you earn every friend and every kind word. That kind of trust doesn't split or warp with the seasons.\"",
                "Marcus says warmly. \"The workshop hears your name each morning now. That's legacy — built slow, like anything worth keeping.\"",
                "Marcus taps a finished plank. \"Steady hands build steady reputations. Yours is the kind Bloomtown will lean on for years.\"",
                "Marcus smiles. \"Folk mention you when they talk about who they trust with real work. That means more than any title.\"",
            ],
            (NpcEntityIds.Marcus, false) =>
            [
                "Marcus nods with quiet respect. \"You've become part of this village's story now — folk will speak of you long after today's work is done.\"",
                "Marcus rests a hand on the bench. \"Trusted neighbors like you shape what Bloomtown becomes. I'm glad I saw it happen.\"",
                "Marcus says plainly. \"Your name comes up in the workshop more than you know — always with respect.\"",
                "Marcus adds warmly. \"Good craft takes patience. So does belonging. You've shown both.\"",
            ],

            (NpcEntityIds.Eleanor, true) =>
            [
                "Eleanor settles into her porch chair with quiet pride. \"You've become part of Bloomtown's story now — and I've taught long enough to know which stories last.\"",
                "Eleanor's eyes soften. \"I watched you earn every friend and every kind word. That's the kind of character villages build their trust on.\"",
                "Eleanor says warmly. \"The porch hears your name each afternoon now. That's legacy, dear — a life remembered with fondness.\"",
                "Eleanor murmurs. \"I used to teach children what kindness looked like. Now the whole village learns it from how you live here.\"",
                "Eleanor takes your hand briefly. \"Old teachers recognize lasting things. Your place in Bloomtown is one of them.\"",
            ],
            (NpcEntityIds.Eleanor, false) =>
            [
                "Eleanor nods with quiet respect. \"You've become part of this village's story now — folk will speak of you long after today's work is done.\"",
                "Eleanor pats your arm gently. \"Trusted neighbors like you shape what Bloomtown becomes. I'm glad I lived long enough to see it.\"",
                "Eleanor smiles. \"The porch is quieter when you're not here — and brighter when you are. The village feels the same way.\"",
                "Eleanor adds softly. \"Some people pass through. You stayed — and Bloomtown is better for it.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetVillagePillarAcknowledgmentLine(
        uint npcEntityId,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (!SocialLegacyConfig.IsEligibleForVillagePillarAcknowledgment(npcEntityId))
            return null;

        string[] lines = (npcEntityId, villageNoticedMemory) switch
        {
            (NpcEntityIds.Harold, true) =>
            [
                "Harold stands straighter at the well. \"Village Pillar — that's what folk call you now, and I won't argue. Bloomtown stands on neighbors like you.\"",
                "Harold tips his hat with solemn warmth. \"I've seen generations come and go by this well. You're among the few the village will speak of with pride for years to come.\"",
                "Harold murmurs with elder's authority. \"Pillars aren't loud, friend. They hold. You've held this village together in ways folk may never fully say aloud — but they feel it.\"",
            ],
            (NpcEntityIds.Harold, false) =>
            [
                "Harold nods slowly. \"They call you a Village Pillar now — and as your elder, I say the title fits. Bloomtown is steadier because you're here.\"",
                "Harold says quietly. \"Not everyone earns a name that outlasts the season. Yours will. That's what pillars do.\"",
            ],

            (NpcEntityIds.Greta, true) =>
            [
                "Greta beams with tears in her eyes. \"Village Pillar, love — my parlor's heard that title spoken with real reverence. You are Bloomtown at its best.\"",
                "Greta fusses over you with extra care. \"Guests travel far and still ask after the pillars of this village. Your name is always first on their lips.\"",
                "Greta squeezes both your hands. \"An innkeeper knows who holds a community together. You're one of them, love — and my hearth has never been prouder.\"",
            ],
            (NpcEntityIds.Greta, false) =>
            [
                "Greta smiles with deep warmth. \"Village Pillar — folk say it at my table like a blessing. You've earned every syllable, love.\"",
                "Greta whispers. \"Some guests come hoping to meet the neighbors Bloomtown is built on. They're hoping to meet you.\"",
            ],

            (NpcEntityIds.Elsie, true) =>
            [
                "Elsie smiles with quiet reverence. \"Village Pillar — the garden knew your kindness long before the lanes named it, but Bloomtown names it proudly now.\"",
                "Elsie says gently. \"Pillars aren't loud, dear — they grow. You've grown here, season by season, until the whole village leans on your trust.\"",
                "Elsie leans on the gate. \"Neighbors speak that title with real fondness — and the beds have heard your name with respect for a long while.\"",
            ],
            (NpcEntityIds.Elsie, false) =>
            [
                "Elsie nods with deep warmth. \"Village Pillar — folk say it like a blessing at the garden gate. You've earned every word.\"",
                "Elsie murmurs. \"Bloomtown is steadier because you rooted here — that's what pillars do.\"",
            ],

            (NpcEntityIds.Rowan, true) =>
            [
                "Rowan's voice drops to something reverent. \"Village Pillar — the old tales speak of neighbors like you. I'm honored to tell yours while I still can.\"",
                "Rowan sets aside his carving. \"Pillars don't seek the spotlight. But Bloomtown shines brighter because you're in its story — and that story will outlast us both.\"",
                "Rowan says softly. \"Someday a child on this bench will hear your name spoken the way we speak of founders and keepers. That's legacy at its truest.\"",
            ],
            (NpcEntityIds.Rowan, false) =>
            [
                "Rowan nods with quiet gravity. \"Village Pillar — not a title handed out lightly. Bloomtown chose you, and chose well.\"",
                "Rowan murmurs. \"The best stories aren't about heroes. They're about neighbors who stayed. You're one of ours now — permanently.\"",
            ],

            (NpcEntityIds.Marcus, true) =>
            [
                "Marcus runs a hand over finished wood. \"Village Pillar — good timber and good people both become the frame everything else rests on. You're that frame now.\"",
                "Marcus says with plain warmth. \"The workshop hears that title spoken with respect. So do I. You've built something here that will last.\"",
                "Marcus nods firmly. \"Pillars don't crack under weight — they carry it. Bloomtown leans on you, and you've never buckled.\"",
            ],
            (NpcEntityIds.Marcus, false) =>
            [
                "Marcus smiles with quiet pride. \"Village Pillar — folk say it like they say your name: with respect earned over years.\"",
                "Marcus adds plainly. \"What you've built here can't be planed away. The village knows it. So do I.\"",
            ],

            (NpcEntityIds.Eleanor, true) =>
            [
                "Eleanor's voice trembles with pride. \"Village Pillar, dear — I taught children what lasting character looked like. You became the lesson.\"",
                "Eleanor takes your hands in both of hers. \"Bloomtown's pillars are the people who stayed, cared, and made others feel they belonged. You are all of that.\"",
                "Eleanor murmurs from her porch. \"I've watched this village long enough to know its foundations. You're one of them now — and I'm so glad I lived to see it.\"",
            ],
            (NpcEntityIds.Eleanor, false) =>
            [
                "Eleanor smiles with deep fondness. \"Village Pillar — the title suits you, dear. Bloomtown is gentler and steadier because you rooted here.\"",
                "Eleanor pats your arm. \"Some legacies are quiet. Yours is the kind the whole village feels when they walk the lanes.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetLegacyAmbientComment(
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (villageNoticedMemory, focusCloseFriendNpcIds.Count >= 3) switch
        {
            (true, true) =>
            [
                "A neighbor murmurs with quiet pride, \"You've become part of Bloomtown's story — folk will remember your name here long after today.\"",
                "Overheard: \"That's one of the village pillars now — not just trusted, but woven into how Bloomtown feels.\"",
                "Someone at the lane edge says, \"People speak of your journey here like a story worth telling — and they're right.\"",
                $"A villager tips their head. \"{FormatSampleName(focusCloseFriendNpcIds, variationSeed)} isn't the only one who remembers — the whole village carries your legacy now.\"",
                "Overheard from the square: \"Bloomtown's lucky to have neighbors who stay and build something lasting.\"",
                "A quiet elder by the well murmurs, \"That one's a pillar of the village now — you can hear it in how folk say their name.\"",
                "Overheard: \"Years of kindness add up to something the whole community feels — and we feel it every day.\"",
                "Someone calls warmly across the lane, \"Bloomtown remembers who stayed. That neighbor stayed — and we're grateful.\"",
            ],
            (true, false) =>
            [
                "A villager remarks softly, \"You've become part of this village's story — neighbors speak your name with real fondness.\"",
                "Overheard: \"Trusted folk like you leave a mark — Bloomtown feels different because you're here.\"",
                "Someone calls warmly, \"That's someone the village will remember — not just today, but seasons from now.\"",
                "A passerby nods with respect. \"Your journey here isn't finished — but what you've built already means something to all of us.\"",
            ],
            (false, _) =>
            [
                "A passerby murmurs, \"You've become part of Bloomtown's story now — folk greet you like someone who belongs.\"",
                "Overheard: \"Neighbors speak of your journey here with respect — that kind of standing lasts.\"",
                "A villager nods your way. \"Trusted neighbors shape what a village becomes — you're doing that here.\"",
                "Someone says quietly, \"Bloomtown remembers who showed up and stayed. That's legacy, that is.\"",
                "Overheard at the market edge: \"That neighbor's been here long enough that the village speaks their name like family.\"",
                "A villager smiles as you pass. \"Good to see you — folk talk about your kindness like it's part of the scenery now.\"",
            ],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string FormatSampleName(IReadOnlyList<uint> npcIds, uint variationSeed)
    {
        if (npcIds.Count == 0)
            return "someone";

        return NpcNameLookup.GetDisplayNameOrDefault(npcIds[(int)(variationSeed % (uint)npcIds.Count)]);
    }
}