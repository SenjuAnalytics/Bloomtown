using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Contribution;
using Serilog;

namespace Bloomtown.Server.Simulation.Contribution;

/// <summary>
/// Tracks village contribution score, titles, and shop bonuses for connected players.
/// </summary>
public sealed class VillageContributionService
{
    private readonly PlayerEconomyService _economyService;

    public VillageContributionService(PlayerEconomyService economyService)
    {
        _economyService = economyService;
    }

    public PlayerVillageContribution GetContribution(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return new PlayerVillageContribution(playerEntityId, 0, VillageTitle.Newcomer);
        }

        return new PlayerVillageContribution(
            playerEntityId,
            economy.VillageContributionScore,
            economy.VillageTitle);
    }

    /// <summary>
    /// Adds contribution points after a successful communal project donation.
    /// Returns an optional message suffix when the title changes.
    /// </summary>
    public string AddContribution(uint playerEntityId, int amount)
    {
        if (amount <= 0 || !_economyService.TryGetState(playerEntityId, out var economy))
            return string.Empty;

        var previousTitle = economy.VillageTitle;
        economy.VillageContributionScore += amount;
        economy.VillageTitle = VillageTitleCalculator.GetTitle(economy.VillageContributionScore);

        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Player {PlayerId} village contribution +{Amount} (total {Score}, title {Title}).",
            playerEntityId,
            amount,
            economy.VillageContributionScore,
            VillageTitleDisplay.GetName(economy.VillageTitle));

        if (economy.VillageTitle == previousTitle)
        {
            return $" Village contribution +{amount} (total {economy.VillageContributionScore}).";
        }

        Log.Information(
            "Player {PlayerId} village title promoted: {PreviousTitle} -> {NewTitle}.",
            playerEntityId,
            VillageTitleDisplay.GetName(previousTitle),
            VillageTitleDisplay.GetName(economy.VillageTitle));

        return
            $" Village contribution +{amount} (total {economy.VillageContributionScore}). New village title: {VillageTitleDisplay.GetName(economy.VillageTitle)}!";
    }

    /// <summary>
    /// Grants a small political bonus to high-ranking villagers after a project contribution.
    /// </summary>
    public string ApplyPoliticalContributionBonus(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return string.Empty;

        // Respected Villager and Elder Candidate earn extra village reputation for communal work.
        if (economy.VillageTitle < VillageTitle.RespectedVillager)
            return string.Empty;

        var bonus = VillageContributionConfig.PoliticalContributionReputationBonus;
        economy.VillageReputation += bonus;
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Political influence bonus: player {PlayerId} ({Title}) +{Bonus} village reputation for communal contribution.",
            playerEntityId,
            VillageTitleDisplay.GetName(economy.VillageTitle),
            bonus);

        return $" Political influence bonus: +{bonus} village reputation.";
    }

    public static float GetBuyPriceMultiplier(VillageTitle title)
    {
        return title switch
        {
            VillageTitle.RespectedVillager => VillageContributionConfig.RespectedVillagerBuyMultiplier,
            VillageTitle.ElderCandidate => VillageContributionConfig.ElderCandidateBuyMultiplier,
            _ => 1f,
        };
    }
}