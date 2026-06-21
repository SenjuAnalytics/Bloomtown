namespace Bloomtown.Shared.Goals;

/// <summary>
/// Naturally detected legacy identity based on how the player engages with Bloomtown.
/// Not chosen upfront — emerges from projects, community help, or social bonds.
/// </summary>
public enum LegacyArchetype : byte
{
    None = 0,
    Builder = 1,
    Caretaker = 2,
    Connector = 3,
}