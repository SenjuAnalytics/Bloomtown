using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Legacy;

/// <summary>
/// Builds player legacy context and gates rare community recognition moments.
/// </summary>
public sealed class PlayerLegacyService
{
    private readonly PlayerEconomyService _economyService;
    private readonly NpcMemoryService _memoryService;
    private readonly CommunityProjectService _communityProjectService;
    private readonly CommunityReputationService? _communityReputationService;
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<uint, long> _lastInteractionRecognitionGameMinute = new();
    private readonly Dictionary<uint, long> _lastAmbientRecognitionGameMinute = new();

    public PlayerLegacyService(
        PlayerEconomyService economyService,
        NpcMemoryService memoryService,
        CommunityProjectService communityProjectService,
        WorldTimeSystem worldTime,
        CommunityReputationService? communityReputationService = null)
    {
        _economyService = economyService;
        _memoryService = memoryService;
        _communityProjectService = communityProjectService;
        _communityReputationService = communityReputationService;
        _worldTime = worldTime;
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _lastInteractionRecognitionGameMinute.Remove(playerEntityId);
        _lastAmbientRecognitionGameMinute.Remove(playerEntityId);
    }

    public PlayerLegacyContext BuildContext(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return new PlayerLegacyContext
            {
                Markers = PlayerLegacyMarker.None,
                VillageTitle = VillageTitle.Newcomer,
                VillageContributionScore = 0,
            };
        }

        var villageMemories = _memoryService.GetMemoriesForNpc(
            playerEntityId,
            NpcMemoryConfig.VillageWideNpcEntityId);

        var completedContributions = _communityProjectService.GetPlayerCompletedProjectContributions(playerEntityId);

        return PlayerLegacyConfig.BuildContext(
            economy.VillageTitle,
            economy.VillageContributionScore,
            villageMemories,
            completedContributions);
    }

    public bool TryGetElderRecognitionResponse(
        uint playerEntityId,
        uint npcEntityId,
        NpcInteractionKind kind,
        PlayerLegacyContext context,
        uint variationSeed,
        out string? line)
    {
        line = null;

        if (!PlayerLegacyConfig.IsElderVoiceNpc(npcEntityId) || !context.HasRecognition)
            return false;

        if (!TryConsumeInteractionRecognitionCooldown(playerEntityId))
            return false;

        if (!PlayerLegacyConfig.ShouldTriggerInteractionRecognition(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        line = PlayerLegacyConfig.TryGetElderRecognitionLine(kind, context, variationSeed);
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var marker = PlayerLegacyConfig.TrySelectRecognitionMarker(context);
        Log.Information(
            "Legacy elder recognition ({Marker}) from {NpcName} to player {PlayerId} on {Interaction}: \"{Line}\"",
            marker,
            NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
            playerEntityId,
            kind,
            line);

        return true;
    }

    public bool TryGetAmbientRecognitionComment(
        uint playerEntityId,
        bool elderNearby,
        PlayerLegacyContext context,
        uint variationSeed,
        out string comment,
        out uint speakerEntityId)
    {
        comment = string.Empty;
        speakerEntityId = 0;

        if (!context.HasRecognition)
            return false;

        if (!TryConsumeAmbientRecognitionCooldown(playerEntityId))
            return false;

        if (!PlayerLegacyConfig.ShouldTriggerAmbientRecognition(playerEntityId, _worldTime.TotalGameMinutes))
            return false;

        string? line;
        if (elderNearby)
        {
            line = PlayerLegacyConfig.TryGetElderAmbientLine(context, variationSeed);
            if (!string.IsNullOrWhiteSpace(line))
            {
                comment = line;
                speakerEntityId = PlayerLegacyConfig.ElderVoiceNpcEntityId;
            }
        }
        else
        {
            line = PlayerLegacyConfig.TryGetVillageWideAmbientLine(context, variationSeed);
            if (!string.IsNullOrWhiteSpace(line))
            {
                comment = line;
                speakerEntityId = 0;
            }
        }

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var marker = PlayerLegacyConfig.TrySelectRecognitionMarker(context);
        Log.Information(
            "Legacy ambient recognition ({Marker}, elderNearby={ElderNearby}) to player {PlayerId}: \"{Comment}\"",
            marker,
            elderNearby,
            playerEntityId,
            comment);

        return true;
    }

    public string? FormatCommunityRecognitionStatus(uint playerEntityId)
    {
        var context = BuildContext(playerEntityId);
        return PlayerLegacyConfig.FormatCommunityRecognitionStatus(context);
    }

    public string? FormatCommunityParticipationStatus(uint playerEntityId) =>
        _communityReputationService?.FormatParticipationStatus(playerEntityId);

    private bool TryConsumeInteractionRecognitionCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastInteractionRecognitionGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < PlayerLegacyConfig.InteractionRecognitionCooldownGameMinutes)
        {
            return false;
        }

        _lastInteractionRecognitionGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    private bool TryConsumeAmbientRecognitionCooldown(uint playerEntityId)
    {
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastAmbientRecognitionGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < PlayerLegacyConfig.AmbientRecognitionCooldownGameMinutes)
        {
            return false;
        }

        _lastAmbientRecognitionGameMinute[playerEntityId] = currentMinute;
        return true;
    }
}