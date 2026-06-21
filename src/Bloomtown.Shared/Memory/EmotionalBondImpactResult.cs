namespace Bloomtown.Shared.Memory;

/// <summary>Outcome of applying emotional-bond benefits during an interaction or bonding action.</summary>
public readonly record struct EmotionalBondImpactResult(
    bool AppliedRecovery,
    float MoodBonus,
    float SocialBonus,
    IReadOnlyList<string> AppendixLines)
{
    public static EmotionalBondImpactResult None { get; } = new(false, 0f, 0f, Array.Empty<string>());
}