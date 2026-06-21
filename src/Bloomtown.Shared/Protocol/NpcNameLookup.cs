namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Case-insensitive NPC name to entity id resolution for interaction commands.
/// </summary>
public static class NpcNameLookup
{
    private static readonly Dictionary<string, uint> NamesByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["elsie"] = NpcEntityIds.Elsie,
        ["tom"] = NpcEntityIds.Tom,
        ["mira"] = NpcEntityIds.Mira,
        ["harold"] = NpcEntityIds.Harold,
        ["greta"] = NpcEntityIds.Greta,
        ["nora"] = NpcEntityIds.Nora,
        ["elias"] = NpcEntityIds.Elias,
        ["ben"] = NpcEntityIds.Ben,
        ["lila"] = NpcEntityIds.Lila,
        ["rowan"] = NpcEntityIds.Rowan,
        ["marcus"] = NpcEntityIds.Marcus,
        ["eleanor"] = NpcEntityIds.Eleanor,
    };

    private static readonly Dictionary<uint, string> DisplayNamesById = new()
    {
        [NpcEntityIds.Elsie] = "Elsie",
        [NpcEntityIds.Tom] = "Tom",
        [NpcEntityIds.Mira] = "Mira",
        [NpcEntityIds.Harold] = "Harold",
        [NpcEntityIds.Greta] = "Greta",
        [NpcEntityIds.Nora] = "Nora",
        [NpcEntityIds.Elias] = "Elias",
        [NpcEntityIds.Ben] = "Ben",
        [NpcEntityIds.Lila] = "Lila",
        [NpcEntityIds.Rowan] = "Rowan",
        [NpcEntityIds.Marcus] = "Marcus",
        [NpcEntityIds.Eleanor] = "Eleanor",
    };

    public static string KnownNamesList => string.Join(", ", NamesByKey.Keys.OrderBy(name => name));

    public static bool TryResolveEntityId(string name, out uint entityId)
    {
        return NamesByKey.TryGetValue(name.Trim(), out entityId);
    }

    public static bool TryGetDisplayName(uint entityId, out string displayName)
    {
        return DisplayNamesById.TryGetValue(entityId, out displayName!);
    }

    public static string GetDisplayNameOrDefault(uint entityId)
    {
        return TryGetDisplayName(entityId, out var displayName) ? displayName : $"NPC {entityId}";
    }
}