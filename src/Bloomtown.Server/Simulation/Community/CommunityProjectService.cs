using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Contribution;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Manages communal project progress, contributions, and completion rewards.
/// </summary>
public sealed class CommunityProjectService
{
    private readonly CommunityProjectRepository _repository;
    private readonly CommunityProjectDefinitionRepository _definitionRepository;
    private readonly PlayerRepository _playerRepository;
    private readonly PlayerEconomyService _economyService;
    private readonly VillageMilestoneService _milestoneService;
    private readonly VillageProjectStateService _projectStateService;
    private readonly VillageContributionService _contributionService;
    private readonly NpcMemoryService? _memoryService;
    private readonly PlayerNpcRelationshipService? _relationshipService;
    private PlayerLongTermGoalService? _longTermGoalService;
    private readonly Dictionary<byte, CommunityProjectState> _projects = new();

    public CommunityProjectService(
        CommunityProjectRepository repository,
        CommunityProjectDefinitionRepository definitionRepository,
        PlayerRepository playerRepository,
        PlayerEconomyService economyService,
        VillageMilestoneService milestoneService,
        VillageProjectStateService projectStateService,
        VillageContributionService contributionService,
        NpcMemoryService? memoryService = null,
        PlayerNpcRelationshipService? relationshipService = null)
    {
        _repository = repository;
        _definitionRepository = definitionRepository;
        _playerRepository = playerRepository;
        _economyService = economyService;
        _milestoneService = milestoneService;
        _projectStateService = projectStateService;
        _contributionService = contributionService;
        _memoryService = memoryService;
        _relationshipService = relationshipService;
    }

    public void ConfigureLongTermGoalService(PlayerLongTermGoalService longTermGoalService) =>
        _longTermGoalService = longTermGoalService;

    /// <summary>
    /// Loads player-proposed project definitions from persistence into the registry.
    /// </summary>
    public async Task LoadDynamicDefinitionsAsync()
    {
        var records = await _definitionRepository.GetAllAsync();
        foreach (var record in records)
        {
            if (CommunityProjectRegistry.TryGet(record.ProjectId, out _))
                continue;

            var definition = new CommunityProjectDefinition
            {
                ProjectId = record.ProjectId,
                Slug = record.Slug,
                Name = record.Name,
                Description = record.Description,
                Requirements = CommunityProjectDefinitionRepository.DeserializeRequirements(record.RequirementsJson),
            };

            CommunityProjectRegistry.Register(definition);
        }

        if (records.Count > 0)
        {
            Log.Information(
                "Loaded {DefinitionCount} dynamic community project definition(s) from database.",
                records.Count);
        }
    }

    /// <summary>
    /// Registers a new player-proposed project and initializes its active state.
    /// </summary>
    public async Task<byte> RegisterDynamicProjectAsync(
        string slug,
        string name,
        string description,
        IReadOnlyDictionary<ItemType, int> requirements)
    {
        var projectId = CommunityProjectRegistry.GetNextProjectId();
        var definition = new CommunityProjectDefinition
        {
            ProjectId = projectId,
            Slug = slug,
            Name = name,
            Description = description,
            Requirements = requirements,
        };

        CommunityProjectRegistry.Register(definition);
        await _definitionRepository.SaveAsync(definition, isBuiltin: false);

        var state = new CommunityProjectState { ProjectId = projectId };
        _projects[projectId] = state;
        await SaveProjectAsync(state);

        Log.Information(
            "Registered dynamic community project {ProjectName} ({ProjectSlug}) as id {ProjectId}.",
            name,
            slug,
            projectId);

        return projectId;
    }

    public async Task LoadAsync()
    {
        var statusRecords = await _repository.GetAllStatusAsync();
        var progressRecords = await _repository.GetAllProgressAsync();
        var contributorRecords = await _repository.GetAllContributorsAsync();

        foreach (var definition in CommunityProjectRegistry.All)
        {
            var state = new CommunityProjectState { ProjectId = definition.ProjectId };

            var status = statusRecords.FirstOrDefault(record => record.ProjectId == definition.ProjectId);
            if (status is not null)
            {
                state.Status = (CommunityProjectStatus)status.Status;
                state.CompletedAtUtc = status.CompletedAtUtc;
            }

            foreach (var progress in progressRecords.Where(record => record.ProjectId == definition.ProjectId))
                state.Progress[progress.ItemType] = progress.CurrentQuantity;

            foreach (var contributor in contributorRecords.Where(record => record.ProjectId == definition.ProjectId))
                state.ContributorTotals[contributor.PlayerEntityId] = contributor.TotalContributed;

            _projects[definition.ProjectId] = state;
        }

        Log.Information("Loaded {ProjectCount} communal project(s) from database.", _projects.Count);
    }

    public string FormatProjectList()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Community Projects:");

        foreach (var definition in CommunityProjectRegistry.All)
        {
            if (!_projects.TryGetValue(definition.ProjectId, out var state))
                continue;

            var statusLabel = state.Status == CommunityProjectStatus.Completed ? "COMPLETED" : "ACTIVE";
            builder.AppendLine();
            builder.AppendLine($"[{statusLabel}] {definition.Name} ({definition.Slug})");
            builder.AppendLine($"  {definition.Description}");

            foreach (var (itemType, required) in definition.Requirements.OrderBy(pair => pair.Key))
            {
                var current = state.Progress.GetValueOrDefault(itemType, 0);
                var itemName = ItemDatabase.GetDisplayName(itemType);
                builder.AppendLine($"  {itemName}: {current}/{required}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    public CommunityProjectResponse Contribute(
        uint playerEntityId,
        byte projectId,
        ItemType itemType,
        byte quantity)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.EconomyUnavailable,
                "Player inventory is unavailable.");
        }

        if (!CommunityProjectRegistry.TryGet(projectId, out var definition))
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.UnknownProject,
                $"Unknown project. Known projects: {CommunityProjectNameLookup.KnownProjectsList}.");
        }

        if (!_projects.TryGetValue(projectId, out var state))
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.UnknownProject,
                "Project state is unavailable.");
        }

        if (state.Status == CommunityProjectStatus.Completed)
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.ProjectCompleted,
                $"{definition.Name} is already completed.");
        }

        if (itemType == 0)
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.UnknownItem,
                "Unknown item type.");
        }

        if (quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (!definition.Requirements.ContainsKey(itemType))
        {
            var itemName = ItemDatabase.GetDisplayName(itemType);
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.ItemNotNeeded,
                $"{definition.Name} does not need {itemName}.");
        }

        var required = definition.Requirements[itemType];
        var current = state.Progress.GetValueOrDefault(itemType, 0);
        var stillNeeded = Math.Max(0, required - current);

        if (stillNeeded == 0)
        {
            var itemName = ItemDatabase.GetDisplayName(itemType);
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.ItemNotNeeded,
                $"{definition.Name} already has enough {itemName}.");
        }

        if (!economy.Inventory.HasItem(itemType, quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(itemType);
            var owned = economy.Inventory.GetItemCount(itemType);
            return Fail(
                CommunityProjectRequestKind.Contribute,
                CommunityProjectFailureReason.NotEnoughItems,
                $"Not enough {itemName}. You have {owned}, tried to contribute {quantity}.");
        }

        var accepted = Math.Min(quantity, Math.Min(stillNeeded, economy.Inventory.GetItemCount(itemType)));
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var progressBonus = 0;
        var elsieProjectBonus = false;
        var tomWoodProjectBonus = false;
        var haroldCommunityProjectBonus = false;
        var haroldElderInfluenceBonus = false;
        var respectedContributionBonus = false;
        var haroldSocialInfluenceBacking = false;
        var elsieSocialInfluenceBacking = false;
        var tomSocialInfluenceBacking = false;
        var noraSocialInfluenceBacking = false;
        var eliasSocialInfluenceBacking = false;
        var benSocialInfluenceBacking = false;
        var lilaSocialInfluenceBacking = false;
        var rowanSocialInfluenceBacking = false;
        var marcusSocialInfluenceBacking = false;
        var eleanorSocialInfluenceBacking = false;
        var socialInfluenceBackingBonus = 0;
        if (stillNeeded > accepted)
        {
            if (_memoryService?.HasHaroldSocialInfluenceProjectBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeHaroldSocialInfluenceProjectBacking(playerEntityId, out var haroldBackingBonus))
            {
                socialInfluenceBackingBonus = haroldBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    haroldBackingBonus);
                haroldSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasElsieSocialInfluenceGardenBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsElsieThemedProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeElsieSocialInfluenceGardenBacking(playerEntityId, out var elsieBackingBonus))
            {
                socialInfluenceBackingBonus = elsieBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    elsieBackingBonus);
                elsieSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasTomSocialInfluenceLumberBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsTomWoodProjectContribution(itemType)
                && _memoryService.TryConsumeTomSocialInfluenceLumberBacking(playerEntityId, out var tomBackingBonus))
            {
                socialInfluenceBackingBonus = tomBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    tomBackingBonus);
                tomSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasNoraSocialInfluenceHerbalBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsNoraHerbProjectContribution(itemType)
                && _memoryService.TryConsumeNoraSocialInfluenceHerbalBacking(playerEntityId, out var noraBackingBonus))
            {
                socialInfluenceBackingBonus = noraBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    noraBackingBonus);
                noraSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasEliasSocialInfluenceSmithingBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsEliasSmithingProjectContribution(itemType)
                && _memoryService.TryConsumeEliasSocialInfluenceSmithingBacking(playerEntityId, out var eliasBackingBonus))
            {
                socialInfluenceBackingBonus = eliasBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    eliasBackingBonus);
                eliasSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasBenSocialInfluenceGuardBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsBenSecurityProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeBenSocialInfluenceGuardBacking(playerEntityId, out var benBackingBonus))
            {
                socialInfluenceBackingBonus = benBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    benBackingBonus);
                benSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasLilaSocialInfluenceYouthBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsLilaVillageProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeLilaSocialInfluenceYouthBacking(playerEntityId, out var lilaBackingBonus))
            {
                socialInfluenceBackingBonus = lilaBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    lilaBackingBonus);
                lilaSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasRowanSocialInfluenceStoryBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsRowanStoryProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeRowanSocialInfluenceStoryBacking(playerEntityId, out var rowanBackingBonus))
            {
                socialInfluenceBackingBonus = rowanBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    rowanBackingBonus);
                rowanSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasMarcusSocialInfluenceCraftingBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsMarcusCraftingProjectContribution(itemType)
                && _memoryService.TryConsumeMarcusSocialInfluenceCraftingBacking(playerEntityId, out var marcusBackingBonus))
            {
                socialInfluenceBackingBonus = marcusBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    marcusBackingBonus);
                marcusSocialInfluenceBacking = true;
            }
            else if (_memoryService?.HasEleanorSocialInfluenceLegacyBacking(playerEntityId) == true
                && VillageSocialStandingMechanicalConfig.IsEleanorLegacyProjectContribution(projectId, itemType)
                && _memoryService.TryConsumeEleanorSocialInfluenceLegacyBacking(playerEntityId, out var eleanorBackingBonus))
            {
                socialInfluenceBackingBonus = eleanorBackingBonus;
                progressBonus = SocialInfluenceActionConfig.ResolveBackingProgressBonus(
                    stillNeeded,
                    accepted,
                    eleanorBackingBonus);
                eleanorSocialInfluenceBacking = true;
            }
            else if (standingTier == VillageSocialStandingTier.Respected
                && VillageSocialStandingMechanicalConfig.ShouldGrantRespectedContributionProgressBonus(
                    playerEntityId,
                    projectId,
                    accepted))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
                respectedContributionBonus = true;
            }
            else if (VillageSocialStandingMechanicalConfig.IsEligibleForContributionBonus(standingTier)
                && VillageSocialStandingMechanicalConfig.ShouldGrantContributionProgressBonus(
                    playerEntityId,
                    projectId,
                    accepted))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
            }
            else if (VillageSocialStandingMechanicalConfig.IsEligibleForRoleMechanicalBonus(standingTier)
                && VillageSocialStandingMechanicalConfig.IsElsieThemedProjectContribution(projectId, itemType)
                && VillageSocialStandingMechanicalConfig.ShouldGrantElsieProjectProgressBonus(
                    playerEntityId,
                    projectId,
                    accepted,
                    standingTier))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
                elsieProjectBonus = true;
            }
            else if (VillageSocialStandingMechanicalConfig.IsEligibleForRoleMechanicalBonus(standingTier)
                && VillageSocialStandingMechanicalConfig.IsTomWoodProjectContribution(itemType)
                && VillageSocialStandingMechanicalConfig.ShouldGrantTomWoodProjectProgressBonus(
                    playerEntityId,
                    projectId,
                    accepted,
                    standingTier))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
                tomWoodProjectBonus = true;
            }
            else if (VillageSocialStandingMechanicalConfig.IsEligibleForRoleMechanicalBonus(standingTier)
                && VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(projectId, itemType)
                && VillageSocialStandingMechanicalConfig.ShouldGrantHaroldProjectProgressBonus(
                    playerEntityId,
                    projectId,
                    accepted,
                    standingTier))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
                haroldCommunityProjectBonus = true;
            }
            else if (VillageSocialStandingMechanicalConfig.IsEligibleForWellLikedPrestigePrivilege(standingTier)
                && VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(projectId, itemType)
                && VillageSocialStandingMechanicalConfig.ShouldGrantHaroldWellLikedElderInfluenceBonus(
                    playerEntityId,
                    projectId,
                    accepted,
                    standingTier))
            {
                progressBonus = Math.Min(1, stillNeeded - accepted);
                haroldElderInfluenceBonus = true;
            }
        }

        var scoreBonus = VillageSocialStandingMechanicalConfig.GetContributionScoreBonus(standingTier);
        economy.Inventory.RemoveItem(itemType, accepted);
        state.Progress[itemType] = current + accepted + progressBonus;
        state.ContributorTotals[playerEntityId] =
            state.ContributorTotals.GetValueOrDefault(playerEntityId, 0) + accepted + progressBonus;

        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        SaveProjectAsync(state).GetAwaiter().GetResult();

        var contributedItem = ItemDatabase.GetDisplayName(itemType);
        Log.Information(
            "Player {PlayerId} contributed {Quantity} {Item} to project {ProjectName} ({ProjectSlug}).",
            playerEntityId,
            accepted,
            contributedItem,
            definition.Name,
            definition.Slug);

        var message = $"Contributed {accepted} {contributedItem} to {definition.Name}. Progress: {state.Progress[itemType]}/{required} {contributedItem}.";
        message += VillageSocialStandingMechanicalConfig.FormatRoleProjectContributionFeedback(
            standingTier,
            progressBonus,
            elsieProjectBonus,
            tomWoodProjectBonus,
            haroldCommunityProjectBonus);
        if (haroldElderInfluenceBonus)
        {
            message += VillageSocialStandingMechanicalConfig.FormatHaroldWellLikedElderInfluenceFeedback(
                standingTier,
                progressBonus);
        }

        var socialInfluenceBackingTier = ResolveSocialInfluenceBackingTier(socialInfluenceBackingBonus);
        if (haroldSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatProjectBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (elsieSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatGardenBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (tomSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatLumberBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (noraSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatHerbalBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (eliasSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatSmithingBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (benSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatGuardBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (lilaSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatYouthBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (marcusSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatCraftingBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (rowanSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatStoryBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (eleanorSocialInfluenceBacking)
        {
            message += $" {SocialInfluenceActionConfig.FormatLegacyBackingFeedback(
                socialInfluenceBackingBonus,
                socialInfluenceBackingBonus,
                socialInfluenceBackingTier)}";
        }

        if (respectedContributionBonus)
        {
            message += VillageSocialStandingMechanicalConfig.FormatRespectedContributionBonusFeedback(progressBonus);
        }

        message += VillageSocialStandingMechanicalConfig.FormatContributionBonusFeedback(
            standingTier,
            progressBonus,
            scoreBonus);

        if (SocialLegacyConfig.IsLegacyActive(standingTier)
            && SocialLegacyConfig.ShouldShowContributionLegacyFeedback(
                playerEntityId,
                projectId,
                totalGameMinutes: playerEntityId + (uint)accepted,
                variationSeed: playerEntityId + (uint)projectId))
        {
            message += $" {SocialLegacyConfig.FormatContributionLegacyFeedback(playerEntityId + (uint)projectId)}";
        }

        message += _contributionService.AddContribution(playerEntityId, accepted + scoreBonus);
        message += _contributionService.ApplyPoliticalContributionBonus(playerEntityId);

        if (_longTermGoalService is not null)
        {
            var agencySeed = playerEntityId + (uint)accepted;
            _longTermGoalService.RecordProjectContributionAndReconcileAsync(playerEntityId, agencySeed).GetAwaiter().GetResult();

            var builderFeedback = _longTermGoalService.TryGetProjectContributionAgencyFeedback(
                playerEntityId,
                agencySeed + 3);
            if (!string.IsNullOrWhiteSpace(builderFeedback))
                message += $"{Environment.NewLine}{builderFeedback}";

            var influenceFeedback = _longTermGoalService.TryGetInfluenceGainFeedback(
                playerEntityId,
                LegacyArchetype.Builder,
                agencySeed + 5);
            if (!string.IsNullOrWhiteSpace(influenceFeedback))
                message += $"{Environment.NewLine}{influenceFeedback}";

            var personalFeedback = _longTermGoalService.TryGetPersonalAlignedActionFeedback(
                playerEntityId,
                LegacyAlignedActionKind.ProjectContribution,
                agencySeed + 7);
            if (!string.IsNullOrWhiteSpace(personalFeedback))
                message += $"{Environment.NewLine}{personalFeedback}";

            if (_longTermGoalService.TryConsumePendingMilestoneFeedback(
                    playerEntityId,
                    agencySeed,
                    out var goalFeedback)
                && !string.IsNullOrWhiteSpace(goalFeedback))
            {
                message += $"{Environment.NewLine}{goalFeedback}";
            }
        }

        // Trigger: first village project contribution becomes a village-wide memory.
        if (_memoryService is not null
            && _memoryService.OnVillageProjectContributedAsync(playerEntityId).GetAwaiter().GetResult())
        {
            message += " The villagers will remember you helped with a community project.";
        }

        if (IsProjectComplete(definition, state))
        {
            state.Status = CommunityProjectStatus.Completed;
            state.CompletedAtUtc = DateTime.UtcNow;
            SaveProjectAsync(state).GetAwaiter().GetResult();
            _projectStateService.MarkCompleted(definition.ProjectId, state.CompletedAtUtc.Value);
            DistributeCompletionRewardsAsync(definition, state).GetAwaiter().GetResult();
            _milestoneService.ActivateForCompletedProjectAsync(definition.ProjectId).GetAwaiter().GetResult();

            message +=
                $" Project completed! Contributors received +{CommunityProjectConfig.CompletionCoinReward} coins and +{CommunityProjectConfig.CompletionVillageReputationReward} village reputation.";

            Log.Information(
                "Community project {ProjectName} ({ProjectSlug}) completed — village milestone activated.",
                definition.Name,
                definition.Slug);
        }

        return new CommunityProjectResponse(
            true,
            CommunityProjectRequestKind.Contribute,
            CommunityProjectFailureReason.None,
            message);
    }

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId)
    {
        if (_relationshipService is null)
            return VillageSocialStandingTier.Stranger;

        return VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
    }

    private static VillageSocialStandingTier ResolveSocialInfluenceBackingTier(int backingBonus) =>
        backingBonus >= SocialInfluenceActionConfig.WellLikedBackingProgressBonus
            ? VillageSocialStandingTier.WellLiked
            : VillageSocialStandingTier.Respected;

    public async Task SaveAllAsync()
    {
        foreach (var state in _projects.Values)
            await SaveProjectAsync(state);
    }

    /// <summary>
    /// Returns ids of completed projects this player contributed to (for legacy markers).
    /// </summary>
    public IReadOnlyList<byte> GetPlayerCompletedProjectContributions(uint playerEntityId)
    {
        var results = new List<byte>();

        foreach (var (projectId, state) in _projects)
        {
            if (state.Status != CommunityProjectStatus.Completed)
                continue;

            if (state.ContributorTotals.GetValueOrDefault(playerEntityId, 0) <= 0)
                continue;

            results.Add(projectId);
        }

        return results;
    }

    private async Task SaveProjectAsync(CommunityProjectState state)
    {
        if (!CommunityProjectRegistry.TryGet(state.ProjectId, out var definition))
            return;

        var progress = definition.Requirements.Keys
            .Select(itemType => ((int)itemType, state.Progress.GetValueOrDefault(itemType, 0)))
            .ToList();

        var contributors = state.ContributorTotals
            .Select(pair => (pair.Key, pair.Value))
            .ToList();

        await _repository.SaveProjectAsync(
            state.ProjectId,
            (int)state.Status,
            progress,
            contributors,
            state.CompletedAtUtc);
    }

    private static bool IsProjectComplete(CommunityProjectDefinition definition, CommunityProjectState state)
    {
        foreach (var (itemType, required) in definition.Requirements)
        {
            if (state.Progress.GetValueOrDefault(itemType, 0) < required)
                return false;
        }

        return true;
    }

    private async Task DistributeCompletionRewardsAsync(CommunityProjectDefinition definition, CommunityProjectState state)
    {
        foreach (var (playerEntityId, contributed) in state.ContributorTotals)
        {
            if (contributed <= 0)
                continue;

            if (_economyService.TryGetState(playerEntityId, out var economy))
            {
                economy.Coins += CommunityProjectConfig.CompletionCoinReward;
                economy.VillageReputation += CommunityProjectConfig.CompletionVillageReputationReward;
                await _economyService.SavePlayerAsync(playerEntityId);

                Log.Information(
                    "Rewarded online player {PlayerId} for completing {ProjectName}: +{Coins} coins, +{Reputation} village reputation.",
                    playerEntityId,
                    definition.Name,
                    CommunityProjectConfig.CompletionCoinReward,
                    CommunityProjectConfig.CompletionVillageReputationReward);
                continue;
            }

            await _playerRepository.AddCoinsAsync(playerEntityId, CommunityProjectConfig.CompletionCoinReward);
            await _playerRepository.AddVillageReputationAsync(
                playerEntityId,
                CommunityProjectConfig.CompletionVillageReputationReward);

            Log.Information(
                "Rewarded offline player {PlayerId} for completing {ProjectName}: +{Coins} coins, +{Reputation} village reputation.",
                playerEntityId,
                definition.Name,
                CommunityProjectConfig.CompletionCoinReward,
                CommunityProjectConfig.CompletionVillageReputationReward);
        }
    }

    private static CommunityProjectResponse Fail(
        CommunityProjectRequestKind kind,
        CommunityProjectFailureReason reason,
        string message)
    {
        Log.Information("Community project request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new CommunityProjectResponse(false, kind, reason, message);
    }
}