using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Compact, scannable formatting for the player status command.
/// </summary>
public static class VillageSocialStandingStatusFormatter
{
    public static string? FormatMilestonesAndLegacySection(
        VillageSocialStandingTier tier,
        int focusCloseFriendCount,
        bool villageNoticedMemory)
    {
        var milestones = SocialLegacyConfig.GetEarnedMilestones(tier, focusCloseFriendCount);
        var hasLegacy = SocialLegacyConfig.IsLegacyActive(tier);
        if (milestones.Count == 0 && !hasLegacy)
            return null;

        var lines = new List<string> { "── Milestones & Legacy ──" };

        foreach (var milestone in milestones)
        {
            var marker = milestone == SocialMilestoneKind.VillagePillar ? "★" : "✓";
            lines.Add(
                $"  {marker} {SocialLegacyConfig.GetMilestoneDisplayName(milestone)} — "
                + $"{SocialLegacyConfig.GetMilestoneDescription(milestone)}");
        }

        if (hasLegacy)
        {
            var legacy = SocialLegacyConfig.FormatLegacyEffectsStatusCompact(
                tier,
                villageNoticedMemory,
                focusCloseFriendCount);
            if (!string.IsNullOrWhiteSpace(legacy))
                lines.Add(legacy);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string? FormatActiveBenefitsSection(VillageSocialStandingTier tier)
    {
        var mechanical = VillageSocialStandingMechanicalConfig.FormatCompactMechanicalBenefitsHint(tier);
        var npcPerks = VillageSocialStandingMechanicalConfig.FormatCompactNpcPerksHint(tier);
        if (string.IsNullOrWhiteSpace(mechanical) && string.IsNullOrWhiteSpace(npcPerks))
            return null;

        var lines = new List<string> { "── Active Benefits ──" };
        if (!string.IsNullOrWhiteSpace(mechanical))
            lines.Add(mechanical);
        if (!string.IsNullOrWhiteSpace(npcPerks))
            lines.Add(npcPerks);

        return string.Join(Environment.NewLine, lines);
    }

    public static string? FormatWhatYouCanDoSection(VillageSocialStandingTier tier)
    {
        var actions = VillageSocialStandingDialogue.FormatCompactTierActionsHint(tier);
        var influence = SocialInfluenceActionConfig.FormatCompactSocialInfluenceHint(tier);
        if (string.IsNullOrWhiteSpace(actions) && string.IsNullOrWhiteSpace(influence))
            return null;

        var lines = new List<string> { "── What You Can Do Now ──" };
        if (!string.IsNullOrWhiteSpace(actions))
            lines.Add(actions);
        if (!string.IsNullOrWhiteSpace(influence))
            lines.Add(influence);
        if (tier == VillageSocialStandingTier.Respected)
        {
            lines.Add(
                "Locked at Well-liked: +2 call-on backing, Mira trade favors, full hearth recovery, "
                + "contribution score bonus, bonus harvests, and legacy effects.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatCallOnAvailabilitySummary(
        VillageSocialStandingTier tier,
        IReadOnlyList<(uint NpcEntityId, bool Ready, int RemainingMinutes, string? PrivilegeLabel)> entries)
    {
        if (entries.Count == 0)
            return string.Empty;

        var ready = entries.Where(e => e.Ready).Select(e => e.NpcEntityId).ToList();
        var onCooldown = entries.Count - ready.Count;
        var readyNames = ready.Count == 0
            ? "none ready"
            : string.Join(", ", ready.Select(NpcNameLookup.GetDisplayNameOrDefault));

        var summary = ready.Count == 0
            ? $"Call-on: all {entries.Count} NPCs on cooldown ({SocialInfluenceActionConfig.GetTierCooldownLabel(tier)})."
            : $"Call-on: {ready.Count} ready ({readyNames}) · {onCooldown} on cooldown.";

        var activePrivileges = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.PrivilegeLabel))
            .Select(e => $"{NpcNameLookup.GetDisplayNameOrDefault(e.NpcEntityId)}: {e.PrivilegeLabel}")
            .ToList();

        if (activePrivileges.Count > 0)
            summary += $"{Environment.NewLine}  Active backing: {string.Join("; ", activePrivileges)}";

        return summary;
    }

    public static string FormatSocialFavorAvailabilitySummary(
        IReadOnlyList<(uint NpcEntityId, bool Ready, int RemainingMinutes)> entries)
    {
        if (entries.Count == 0)
            return string.Empty;

        var ready = entries.Where(e => e.Ready).Select(e => e.NpcEntityId).ToList();
        var onCooldown = entries.Count - ready.Count;
        if (ready.Count == 0)
            return $"Village favors: all {entries.Count} NPCs on cooldown.";

        var readyNames = string.Join(", ", ready.Select(NpcNameLookup.GetDisplayNameOrDefault));
        return $"Village favors: {ready.Count} ready ({readyNames}) · {onCooldown} on cooldown.";
    }

    public static string? FormatVillageTreatmentSection(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory) =>
        FormatHowTheVillageSeesYouSection(tier, villageNoticedMemory);

    public static string? FormatHowTheVillageSeesYouSection(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        if (tier < VillageSocialStandingTier.Respected)
            return null;

        var lines = new List<string> { "── How the Village Sees You ──" };

        var atmosphere = VillageSocialStandingConfig.FormatVillageAtmosphereHint(tier, villageNoticedMemory);
        var impact = VillageSocialStandingConfig.FormatStandingImpactHint(tier, villageNoticedMemory);
        var broader = VillageSocialStandingConfig.FormatBroaderVillageRecognitionHint(tier, villageNoticedMemory);
        var ordinary = VillageSocialStandingConfig.FormatOrdinaryVillagerAmbientStatusHint(tier);
        var prestige = VillageSocialStandingConfig.FormatPrestigeStatusHint(tier);

        if (!string.IsNullOrWhiteSpace(atmosphere))
            lines.Add(atmosphere);
        if (!string.IsNullOrWhiteSpace(impact))
            lines.Add(impact);
        if (!string.IsNullOrWhiteSpace(broader))
            lines.Add(broader);
        if (!string.IsNullOrWhiteSpace(ordinary))
            lines.Add(ordinary);
        if (!string.IsNullOrWhiteSpace(prestige))
            lines.Add(prestige);

        return lines.Count > 1
            ? string.Join(Environment.NewLine, lines)
            : null;
    }
}