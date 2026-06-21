using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Tracks unlocked village areas and applies their interactions and passive presence checks.
/// </summary>
public sealed class VillageAreaService
{
    private readonly VillageAreaRepository _repository;
    private readonly PlayerEconomyService _economyService;
    private readonly HashSet<VillageArea> _unlocked = new();
    private readonly Dictionary<(uint PlayerId, VillageAreaInteractionKind Kind), DateTime> _cooldowns = new();
    private readonly Dictionary<(uint PlayerId, VillageArea Area), DateTime> _ambientCommentCooldowns = new();

    /// <summary>Fired when a new area opens; used to notify online players.</summary>
    public event Action<string>? AreaUnlocked;

    public VillageAreaService(
        VillageAreaRepository repository,
        PlayerEconomyService economyService)
    {
        _repository = repository;
        _economyService = economyService;
    }

    public async Task LoadAsync()
    {
        _unlocked.Clear();
        var records = await _repository.GetAllAsync();
        foreach (var record in records)
            _unlocked.Add((VillageArea)record.AreaId);

        Log.Information("Loaded {AreaCount} unlocked village area(s).", _unlocked.Count);
    }

    /// <summary>
    /// Unlocks areas whose development-level requirement is now met.
    /// Called on server start (silent) and when the village level advances (announced).
    /// </summary>
    public async Task ReconcileUnlocksAsync(VillageDevelopmentLevel developmentLevel, bool announce)
    {
        foreach (var definition in VillageAreaConfig.All)
        {
            if (_unlocked.Contains(definition.Area))
                continue;

            if (developmentLevel < definition.RequiredLevel)
                continue;

            await UnlockAreaAsync(definition.Area, announce);
        }
    }

    public bool IsUnlocked(VillageArea area) => _unlocked.Contains(area);

    public IReadOnlyList<VillageArea> GetUnlockedAreas() =>
        _unlocked.OrderBy(area => (byte)area).ToList();

    /// <summary>Returns active interactions for an unlocked area.</summary>
    public IReadOnlyList<VillageAreaInteractionDefinition> GetAvailableInteractions(VillageArea area)
    {
        if (!IsUnlocked(area))
            return Array.Empty<VillageAreaInteractionDefinition>();

        return VillageAreaConfig.GetInteractionsForArea(area);
    }

    /// <summary>
    /// Finds the nearest unlocked area whose passive radius contains the given position.
    /// </summary>
    public bool TryGetPassiveAreaAtPosition(float playerX, float playerZ, out VillageAreaDefinition definition)
    {
        definition = null!;
        VillageAreaDefinition? nearest = null;
        var nearestDistance = float.MaxValue;

        foreach (var areaDefinition in VillageAreaConfig.All)
        {
            if (!IsUnlocked(areaDefinition.Area))
                continue;

            var distance = GetDistance(playerX, playerZ, areaDefinition.WorldX, areaDefinition.WorldZ);
            if (distance > VillageAreaConfig.PassiveRadiusMeters || distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = areaDefinition;
        }

        if (nearest is null)
            return false;

        definition = nearest;
        return true;
    }

    /// <summary>Whether the player is inside an unlocked area's passive radius.</summary>
    public bool TryGetUnlockedAreaAtPosition(float playerX, float playerZ, out VillageArea area)
    {
        area = default;
        if (!TryGetPassiveAreaAtPosition(playerX, playerZ, out var definition))
            return false;

        area = definition.Area;
        return true;
    }

    public bool TryConsumeAreaAmbientCommentCooldown(uint playerEntityId, VillageArea area)
    {
        var key = (playerEntityId, area);
        if (_ambientCommentCooldowns.TryGetValue(key, out var lastUsed)
            && DateTime.UtcNow - lastUsed < VillageAreaConfig.AreaAmbientCommentCooldown)
        {
            return false;
        }

        _ambientCommentCooldowns[key] = DateTime.UtcNow;
        return true;
    }

    public string FormatAreaList()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Village Areas:");

        foreach (var definition in VillageAreaConfig.All)
        {
            var unlocked = IsUnlocked(definition.Area);
            var status = unlocked ? "UNLOCKED" : "LOCKED";
            builder.AppendLine();
            builder.AppendLine($"[{status}] {definition.Name}");
            builder.AppendLine($"  {definition.Description}");
            if (unlocked)
            {
                var commands = string.Join(", ", GetAvailableInteractions(definition.Area).Select(i => i.CommandHint));
                builder.AppendLine($"  Interactions: {commands} (within {VillageAreaConfig.InteractionRadiusMeters:F0}m of {definition.WorldX:F0},{definition.WorldZ:F0})");

                var passive = VillageAreaConfig.FormatPassiveSummary(definition);
                if (!string.IsNullOrWhiteSpace(passive))
                    builder.AppendLine($"  Passive effect{passive.Replace(" — passive:", ":")}");
            }
            else
            {
                builder.AppendLine($"  Requires village atmosphere: {VillageAreaConfig.FormatUnlockRequirement(definition.RequiredLevel)}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    public string FormatStatusAreas()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Village areas:");

        foreach (var definition in VillageAreaConfig.All)
            builder.AppendLine($"  {VillageAreaConfig.FormatStatusLine(definition, IsUnlocked(definition.Area))}");

        return builder.ToString().TrimEnd();
    }

    public VillageAreaResponse Interact(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageAreaInteractionKind interaction)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!VillageAreaConfig.TryGetInteraction(interaction, out var interactionDefinition))
        {
            return Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.UnknownInteraction,
                "Unknown village area interaction.");
        }

        if (!VillageAreaConfig.TryGet(interactionDefinition.Area, out var areaDefinition))
        {
            return Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.UnknownInteraction,
                "Unknown village area interaction.");
        }

        if (!IsUnlocked(interactionDefinition.Area))
        {
            return Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.AreaLocked,
                $"{areaDefinition.Name} is not open yet. Grow the village to {VillageAreaConfig.FormatUnlockRequirement(areaDefinition.RequiredLevel)} first.");
        }

        var distance = GetDistance(playerX, playerZ, areaDefinition.WorldX, areaDefinition.WorldZ);
        if (distance > VillageAreaConfig.InteractionRadiusMeters)
        {
            return Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.NotInRange,
                $"{areaDefinition.Name} is too far away ({distance:F1}m). Move within {VillageAreaConfig.InteractionRadiusMeters:F0}m of ({areaDefinition.WorldX:F0}, {areaDefinition.WorldZ:F0}).");
        }

        if (TryGetCooldownFailure(playerEntityId, interaction, out var cooldownFailure))
            return cooldownFailure;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        var socialBefore = economy.SocialNeed;

        if (interactionDefinition.MoodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + interactionDefinition.MoodGain);

        if (interactionDefinition.FatigueReduction > 0f)
            economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - interactionDefinition.FatigueReduction);

        if (interactionDefinition.SocialReduction > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - interactionDefinition.SocialReduction);

        SetCooldown(playerEntityId, interaction);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Player {PlayerId} used {Interaction} at {AreaName} — mood {MoodBefore:F0}->{MoodAfter:F0}, fatigue {FatigueBefore:F0}->{FatigueAfter:F0}, social {SocialBefore:F0}->{SocialAfter:F0}.",
            playerEntityId,
            interactionDefinition.CommandHint,
            areaDefinition.Name,
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue,
            socialBefore,
            economy.SocialNeed);

        var flavorSeed = playerEntityId + (uint)interaction + (uint)_cooldowns.Count;
        var message = BuildInteractionMessage(
            areaDefinition,
            interactionDefinition,
            economy,
            moodBefore,
            fatigueBefore,
            socialBefore,
            flavorSeed);
        return new VillageAreaResponse(true, VillageAreaRequestKind.Interact, VillageAreaFailureReason.None, message);
    }

    private async Task UnlockAreaAsync(VillageArea area, bool announce)
    {
        if (_unlocked.Contains(area))
            return;

        var unlockedAt = DateTime.UtcNow;
        _unlocked.Add(area);
        await _repository.UpsertAsync((byte)area, unlockedAt);

        if (!VillageAreaConfig.TryGet(area, out var definition))
            return;

        Log.Information(
            "Village area {AreaName} unlocked (requires {RequiredLevel}).",
            definition.Name,
            VillageAreaConfig.FormatUnlockRequirement(definition.RequiredLevel));

        if (announce)
        {
            var notification = $"[Village Area] {VillageAreaConfig.GetUnlockFlavor(area)}";
            AreaUnlocked?.Invoke(notification);
        }
    }

    private static string BuildInteractionMessage(
        VillageAreaDefinition areaDefinition,
        VillageAreaInteractionDefinition interactionDefinition,
        PlayerEconomyState economy,
        float moodBefore,
        float fatigueBefore,
        float socialBefore,
        uint flavorSeed)
    {
        var parts = new List<string>
        {
            VillageAreaConfig.PickInteractionFlavor(interactionDefinition, flavorSeed),
        };

        if (economy.Mood > moodBefore)
            parts.Add($"Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0})");

        if (economy.Fatigue < fatigueBefore)
            parts.Add($"Fatigue -{fatigueBefore - economy.Fatigue:F0} (now {economy.Fatigue:F0})");

        if (economy.SocialNeed < socialBefore)
            parts.Add($"Social +{socialBefore - economy.SocialNeed:F0} (now {economy.SocialNeed:F0})");

        return string.Join(" ", parts);
    }

    private bool TryGetCooldownFailure(
        uint playerEntityId,
        VillageAreaInteractionKind interaction,
        out VillageAreaResponse failure)
    {
        var cooldown = VillageAreaConfig.GetInteractionCooldown(interaction);
        var key = (playerEntityId, interaction);
        if (_cooldowns.TryGetValue(key, out var lastUsed) && DateTime.UtcNow - lastUsed < cooldown)
        {
            var remaining = cooldown - (DateTime.UtcNow - lastUsed);
            var commandHint = VillageAreaConfig.TryGetInteraction(interaction, out var definition)
                ? definition.CommandHint
                : "this";
            failure = Fail(
                VillageAreaRequestKind.Interact,
                VillageAreaFailureReason.OnCooldown,
                $"Please wait {remaining.TotalSeconds:F0}s before using '{commandHint}' again.");
            return true;
        }

        failure = default;
        return false;
    }

    private void SetCooldown(uint playerEntityId, VillageAreaInteractionKind interaction)
    {
        _cooldowns[(playerEntityId, interaction)] = DateTime.UtcNow;
    }

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private static VillageAreaResponse Fail(
        VillageAreaRequestKind kind,
        VillageAreaFailureReason reason,
        string message)
    {
        Log.Information("Village area request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new VillageAreaResponse(false, kind, reason, message);
    }
}