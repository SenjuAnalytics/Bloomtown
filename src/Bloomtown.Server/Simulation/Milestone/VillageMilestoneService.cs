using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Milestone;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Milestone;

/// <summary>
/// Tracks unlocked village milestones and applies their interactive effects.
/// </summary>
public sealed class VillageMilestoneService
{
    private readonly VillageMilestoneRepository _repository;
    private readonly CommunityProjectRepository _projectRepository;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService? _needsService;
    private readonly HashSet<VillageMilestone> _unlocked = new();
    private readonly Dictionary<(uint PlayerId, MilestoneInteractionKind Kind), DateTime> _cooldowns = new();

    /// <summary>
    /// Fired when a milestone becomes active; used to notify online players.
    /// </summary>
    public event Action<string>? MilestoneUnlocked;

    public VillageMilestoneService(
        VillageMilestoneRepository repository,
        CommunityProjectRepository projectRepository,
        PlayerEconomyService economyService,
        PlayerNeedsService? needsService = null)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _economyService = economyService;
        _needsService = needsService;
    }

    public async Task LoadAsync()
    {
        var records = await _repository.GetAllAsync();
        foreach (var record in records)
            _unlocked.Add(record.MilestoneId);

        Log.Information("Loaded {MilestoneCount} unlocked village milestone(s).", _unlocked.Count);
    }

    /// <summary>
    /// Ensures milestones are active for any projects already marked completed in the database.
    /// </summary>
    public async Task ReconcileFromCompletedProjectsAsync()
    {
        var statuses = await _projectRepository.GetAllStatusAsync();
        foreach (var status in statuses.Where(record => record.Status == (int)Community.CommunityProjectStatus.Completed))
            await ActivateForCompletedProjectAsync((byte)status.ProjectId, announce: false);
    }

    public bool IsUnlocked(VillageMilestone milestone) => _unlocked.Contains(milestone);

    public string FormatMilestoneList()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Village Milestones:");

        foreach (var definition in VillageMilestoneRegistry.All)
        {
            var unlocked = IsUnlocked(definition.Milestone);
            var status = unlocked ? "ACTIVE" : "LOCKED";
            builder.AppendLine();
            builder.AppendLine($"[{status}] {definition.Name}");
            builder.AppendLine($"  {definition.Description}");
            if (unlocked)
                builder.AppendLine($"  Interaction: {definition.InteractionHint} (near {definition.WorldX:F0},{definition.WorldZ:F0})");
            else
                builder.AppendLine("  Complete the matching community project to unlock.");
        }

        return builder.ToString().TrimEnd();
    }

    public async Task ActivateForCompletedProjectAsync(byte projectId, bool announce = true)
    {
        if (!VillageMilestoneRegistry.TryGetForProject(projectId, out var definition))
            return;

        if (_unlocked.Contains(definition.Milestone))
            return;

        var unlockedAt = DateTime.UtcNow;
        _unlocked.Add(definition.Milestone);
        await _repository.UpsertAsync(definition.Milestone, unlockedAt);

        Log.Information(
            "Village milestone {MilestoneName} unlocked (project {ProjectId}).",
            definition.Name,
            projectId);

        if (announce)
        {
            var npcLine = VillageProjectNpcDialogue.GetCompletionBroadcast(projectId);
            MilestoneUnlocked?.Invoke(
                $"[Milestone] {definition.Name} is now active! {definition.UnlockAnnouncement} A villager remarks: \"{npcLine}\"");
        }
    }

    public MilestoneResponse Interact(
        uint playerEntityId,
        float playerX,
        float playerZ,
        MilestoneInteractionKind interaction)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!VillageMilestoneRegistry.TryGetForInteraction(interaction, out var definition))
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.UnknownInteraction,
                "Unknown milestone interaction.");
        }

        if (!IsUnlocked(definition.Milestone))
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.MilestoneLocked,
                $"{definition.Name} is not unlocked yet. Complete the matching community project first.");
        }

        var distance = GetDistance(playerX, playerZ, definition.WorldX, definition.WorldZ);
        if (distance > VillageMilestoneConfig.InteractionRadiusMeters)
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.NotInRange,
                $"{definition.Name} is too far away ({distance:F1}m). Move within {VillageMilestoneConfig.InteractionRadiusMeters:F0}m of ({definition.WorldX:F0}, {definition.WorldZ:F0}).");
        }

        return interaction switch
        {
            MilestoneInteractionKind.DrinkWell => HandleDrinkWell(playerEntityId, economy, definition),
            MilestoneInteractionKind.CrossBridge => HandleCrossBridge(playerEntityId, economy, definition),
            MilestoneInteractionKind.CollectStipend => HandleCollectStipend(playerEntityId, economy, definition),
            _ => Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.UnknownInteraction,
                "Unknown milestone interaction."),
        };
    }

    private MilestoneResponse HandleDrinkWell(
        uint playerEntityId,
        PlayerEconomyState economy,
        VillageMilestoneDefinition definition)
    {
        if (TryGetCooldownFailure(playerEntityId, MilestoneInteractionKind.DrinkWell, VillageMilestoneConfig.WellDrinkCooldown, out var cooldownFailure))
            return cooldownFailure;

        if (economy.Energy >= VillageMilestoneConfig.MaxPlayerEnergy)
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.AlreadyFull,
                $"You already feel refreshed (Energy {economy.Energy:F0}/{VillageMilestoneConfig.MaxPlayerEnergy:F0}).");
        }

        // Passive benefit: completed well project boosts drink energy restore.
        var energyRestore = VillageProjectBenefitConfig.GetWellDrinkEnergyRestore(projectComplete: true);
        var before = economy.Energy;
        economy.Energy = Math.Min(
            VillageMilestoneConfig.MaxPlayerEnergy,
            economy.Energy + energyRestore);
        SetCooldown(playerEntityId, MilestoneInteractionKind.DrinkWell);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var restored = economy.Energy - before;
        var wasLow = before <= PlayerEnergyConfig.LowEnergyThreshold;
        Log.Information(
            "Player {PlayerId} drank from the village well (+{Restore:F0} energy with project bonus, now {Energy:F0}{LowNote}).",
            playerEntityId,
            restored,
            economy.Energy,
            wasLow ? ", recovered from low energy" : string.Empty);

        return new MilestoneResponse(
            true,
            MilestoneRequestKind.Interact,
            MilestoneFailureReason.None,
            $"You drink cool water from the well. Energy +{restored:F0} (now {economy.Energy:F0}/{VillageMilestoneConfig.MaxPlayerEnergy:F0}).");
    }

    private MilestoneResponse HandleCrossBridge(
        uint playerEntityId,
        PlayerEconomyState economy,
        VillageMilestoneDefinition definition)
    {
        if (economy.Energy >= VillageMilestoneConfig.MaxPlayerEnergy)
        {
            return Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.AlreadyFull,
                $"You are already energized (Energy {economy.Energy:F0}/{VillageMilestoneConfig.MaxPlayerEnergy:F0}).");
        }

        var before = economy.Energy;
        economy.Energy = Math.Min(
            VillageMilestoneConfig.MaxPlayerEnergy,
            economy.Energy + VillageMilestoneConfig.BridgeCrossEnergyRestore);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var energyGained = economy.Energy - before;
        var fatigueMessage = string.Empty;
        if (_needsService is not null)
        {
            // Passive benefit: crossing the repaired bridge eases fatigue.
            var fatigueReduced = _needsService.ApplyBridgeCrossRelief(
                economy,
                VillageProjectBenefitConfig.BridgeCrossFatigueReduction);
            if (fatigueReduced > 0)
                fatigueMessage = $" Fatigue -{fatigueReduced:F0}.";
        }

        Log.Information(
            "Player {PlayerId} crossed the repaired bridge (+{Restore:F0} energy, now {Energy:F0}{FatigueNote}).",
            playerEntityId,
            energyGained,
            economy.Energy,
            string.IsNullOrEmpty(fatigueMessage) ? string.Empty : ", fatigue eased");

        return new MilestoneResponse(
            true,
            MilestoneRequestKind.Interact,
            MilestoneFailureReason.None,
            $"You cross the sturdy bridge with ease. Energy +{energyGained:F0} (now {economy.Energy:F0}/{VillageMilestoneConfig.MaxPlayerEnergy:F0}).{fatigueMessage}");
    }

    private MilestoneResponse HandleCollectStipend(
        uint playerEntityId,
        PlayerEconomyState economy,
        VillageMilestoneDefinition definition)
    {
        if (TryGetCooldownFailure(playerEntityId, MilestoneInteractionKind.CollectStipend, VillageMilestoneConfig.WarehouseStipendCooldown, out var cooldownFailure))
            return cooldownFailure;

        // Passive benefit: completed warehouse project boosts stipend payout.
        var stipendCoins = VillageProjectBenefitConfig.GetWarehouseStipendCoins(projectComplete: true);
        economy.Coins += stipendCoins;
        SetCooldown(playerEntityId, MilestoneInteractionKind.CollectStipend);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Player {PlayerId} collected warehouse stipend (+{Coins} coins with project bonus, now {TotalCoins}).",
            playerEntityId,
            stipendCoins,
            economy.Coins);

        return new MilestoneResponse(
            true,
            MilestoneRequestKind.Interact,
            MilestoneFailureReason.None,
            $"You collect supplies from the warehouse. +{stipendCoins} coins (now {economy.Coins}).");
    }

    private bool TryGetCooldownFailure(
        uint playerEntityId,
        MilestoneInteractionKind interaction,
        TimeSpan cooldown,
        out MilestoneResponse failure)
    {
        var key = (playerEntityId, interaction);
        if (_cooldowns.TryGetValue(key, out var lastUsed) && DateTime.UtcNow - lastUsed < cooldown)
        {
            var remaining = cooldown - (DateTime.UtcNow - lastUsed);
            failure = Fail(
                MilestoneRequestKind.Interact,
                MilestoneFailureReason.OnCooldown,
                $"Please wait {remaining.TotalSeconds:F0}s before using this again.");
            return true;
        }

        failure = default;
        return false;
    }

    private void SetCooldown(uint playerEntityId, MilestoneInteractionKind interaction)
    {
        _cooldowns[(playerEntityId, interaction)] = DateTime.UtcNow;
    }

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private static MilestoneResponse Fail(MilestoneRequestKind kind, MilestoneFailureReason reason, string message)
    {
        Log.Information("Milestone request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new MilestoneResponse(false, kind, reason, message);
    }
}