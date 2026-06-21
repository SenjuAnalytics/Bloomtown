namespace Bloomtown.Shared.Goals;

/// <summary>
/// Player-earned nudges toward a legacy path through consistent actions — light agency without upfront choice.
/// </summary>
public readonly record struct LegacyArchetypeInfluence(
    int BuilderPoints,
    int CaretakerPoints,
    int ConnectorPoints)
{
    public static LegacyArchetypeInfluence Empty => new(0, 0, 0);

    public int GetPoints(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder => BuilderPoints,
            LegacyArchetype.Caretaker => CaretakerPoints,
            LegacyArchetype.Connector => ConnectorPoints,
            _ => 0,
        };
}