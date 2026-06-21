using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Needs;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class PlayerRepository
{
    private readonly string _connectionString;

    public PlayerRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<PlayerRecord?> GetMostRecentAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                entity_id AS EntityId,
                position_x AS PositionX,
                position_y AS PositionY,
                position_z AS PositionZ,
                rotation_yaw AS RotationYaw,
                last_seen AS LastSeenUtc,
                coins AS Coins,
                village_reputation AS VillageReputation,
                energy AS Energy,
                hunger AS Hunger,
                mood AS Mood,
                fatigue AS Fatigue,
                social_need AS SocialNeed,
                needs_last_game_minute AS NeedsLastGameMinute,
                village_contribution_score AS VillageContributionScore,
                village_title AS VillageTitleId,
                village_position AS VillagePositionId,
                position_assigned_at AS PositionAssignedAtUtc
            FROM players
            ORDER BY last_seen DESC
            LIMIT 1
            """;

        var row = await connection.QuerySingleOrDefaultAsync<PlayerRow>(sql);
        return row?.ToRecord();
    }

    public async Task<int?> GetCoinsAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int?>(
            "SELECT coins FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<uint?> GetMaxEntityIdAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<uint?>("SELECT MAX(entity_id) FROM players");
    }

    public async Task<int?> GetVillageReputationAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int?>(
            "SELECT village_reputation FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<float?> GetEnergyAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<float?>(
            "SELECT energy FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<float?> GetHungerAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<float?>(
            "SELECT hunger FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<float?> GetMoodAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<float?>(
            "SELECT mood FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<float?> GetFatigueAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<float?>(
            "SELECT fatigue FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<float?> GetSocialNeedAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<float?>(
            "SELECT social_need FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<long?> GetNeedsLastGameMinuteAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<long?>(
            "SELECT needs_last_game_minute FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<int?> GetVillageContributionScoreAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int?>(
            "SELECT village_contribution_score FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });
    }

    public async Task<VillageTitle?> GetVillageTitleAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var value = await connection.ExecuteScalarAsync<int?>(
            "SELECT village_title FROM players WHERE entity_id = @EntityId",
            new { EntityId = entityId });

        return value.HasValue ? (VillageTitle)value.Value : null;
    }

    public async Task<(VillagePosition Position, DateTime? AssignedAtUtc)?> GetVillagePositionAsync(uint entityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<PositionRow>(
            """
            SELECT village_position AS VillagePositionId, position_assigned_at AS PositionAssignedAtUtc
            FROM players
            WHERE entity_id = @EntityId
            """,
            new { EntityId = entityId });

        if (row is null)
            return null;

        return ((VillagePosition)row.VillagePositionId, ParseOptionalUtc(row.PositionAssignedAtUtc));
    }

    public async Task<uint?> GetHolderPlayerIdAsync(VillagePosition position)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<uint?>(
            """
            SELECT entity_id
            FROM players
            WHERE village_position = @Position
            ORDER BY position_assigned_at DESC
            LIMIT 1
            """,
            new { Position = (int)position });
    }

    public async Task<int> CountCouncilMembersAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1)
            FROM players
            WHERE village_position IN (@Advisor, @DeputyChief, @Chief)
            """,
            new
            {
                Advisor = (int)VillagePosition.Advisor,
                DeputyChief = (int)VillagePosition.DeputyChief,
                Chief = (int)VillagePosition.Chief,
            });
    }

    public async Task<IReadOnlyList<PlayerContributionSummaryRecord>> GetContributionSummariesAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                entity_id AS EntityId,
                village_contribution_score AS VillageContributionScore,
                village_title AS VillageTitleId,
                village_position AS VillagePositionId
            FROM players
            ORDER BY village_contribution_score DESC, entity_id ASC
            """;

        var rows = await connection.QueryAsync<PlayerContributionSummaryRecord>(sql);
        return rows.ToList();
    }

    public async Task UpdateVillagePositionAsync(
        uint entityId,
        VillagePosition position,
        DateTime? assignedAtUtc)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE players
            SET village_position = @Position,
                position_assigned_at = @PositionAssignedAtUtc
            WHERE entity_id = @EntityId
            """,
            new
            {
                EntityId = entityId,
                Position = (int)position,
                PositionAssignedAtUtc = assignedAtUtc?.ToString("O"),
            });
    }

    public async Task ClearVillagePositionAsync(VillagePosition position)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE players
            SET village_position = 0, position_assigned_at = NULL
            WHERE village_position = @Position
            """,
            new { Position = (int)position });
    }

    public async Task UpdateEconomyAsync(
        uint entityId,
        int coins,
        int villageReputation,
        float energy,
        float hunger,
        float mood,
        float fatigue,
        float socialNeed,
        long needsLastGameMinute,
        int villageContributionScore,
        VillageTitle villageTitle,
        VillagePosition villagePosition,
        DateTime? positionAssignedAtUtc)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE players
            SET
                coins = @Coins,
                village_reputation = @VillageReputation,
                energy = @Energy,
                hunger = @Hunger,
                mood = @Mood,
                fatigue = @Fatigue,
                social_need = @SocialNeed,
                needs_last_game_minute = @NeedsLastGameMinute,
                village_contribution_score = @VillageContributionScore,
                village_title = @VillageTitle,
                village_position = @VillagePosition,
                position_assigned_at = @PositionAssignedAtUtc
            WHERE entity_id = @EntityId
            """,
            new
            {
                EntityId = entityId,
                Coins = coins,
                VillageReputation = villageReputation,
                Energy = energy,
                Hunger = hunger,
                Mood = mood,
                Fatigue = fatigue,
                SocialNeed = socialNeed,
                NeedsLastGameMinute = needsLastGameMinute,
                VillageContributionScore = villageContributionScore,
                VillageTitle = (int)villageTitle,
                VillagePosition = (int)villagePosition,
                PositionAssignedAtUtc = positionAssignedAtUtc?.ToString("O"),
            });
    }

    public async Task AddCoinsAsync(uint entityId, int amount)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE players SET coins = coins + @Amount WHERE entity_id = @EntityId",
            new { EntityId = entityId, Amount = amount });
    }

    public async Task AddVillageReputationAsync(uint entityId, int amount)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE players SET village_reputation = village_reputation + @Amount WHERE entity_id = @EntityId",
            new { EntityId = entityId, Amount = amount });
    }

    public async Task UpsertAsync(PlayerRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO players (
                entity_id, position_x, position_y, position_z, rotation_yaw, last_seen,
                coins, village_reputation, energy, hunger, mood, fatigue, social_need, needs_last_game_minute,
                village_contribution_score, village_title,
                village_position, position_assigned_at)
            VALUES (
                @EntityId, @PositionX, @PositionY, @PositionZ, @RotationYaw, @LastSeenUtc,
                @Coins, @VillageReputation, @Energy, @Hunger, @Mood, @Fatigue, @SocialNeed, @NeedsLastGameMinute,
                @VillageContributionScore, @VillageTitle,
                @VillagePosition, @PositionAssignedAtUtc)
            ON CONFLICT(entity_id) DO UPDATE SET
                position_x = excluded.position_x,
                position_y = excluded.position_y,
                position_z = excluded.position_z,
                rotation_yaw = excluded.rotation_yaw,
                last_seen = excluded.last_seen,
                coins = excluded.coins,
                village_reputation = excluded.village_reputation,
                energy = excluded.energy,
                hunger = excluded.hunger,
                mood = excluded.mood,
                fatigue = excluded.fatigue,
                social_need = excluded.social_need,
                needs_last_game_minute = excluded.needs_last_game_minute,
                village_contribution_score = excluded.village_contribution_score,
                village_title = excluded.village_title,
                village_position = excluded.village_position,
                position_assigned_at = excluded.position_assigned_at
            """;

        await connection.ExecuteAsync(sql, new
        {
            record.EntityId,
            record.PositionX,
            record.PositionY,
            record.PositionZ,
            record.RotationYaw,
            LastSeenUtc = record.LastSeenUtc.ToString("O"),
            record.Coins,
            record.VillageReputation,
            record.Energy,
            record.Hunger,
            record.Mood,
            record.Fatigue,
            record.SocialNeed,
            record.NeedsLastGameMinute,
            record.VillageContributionScore,
            VillageTitle = (int)record.VillageTitle,
            VillagePosition = (int)record.VillagePosition,
            PositionAssignedAtUtc = record.PositionAssignedAtUtc?.ToString("O"),
        });
    }

    private static DateTime? ParseOptionalUtc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private sealed class PositionRow
    {
        public int VillagePositionId { get; init; }
        public string? PositionAssignedAtUtc { get; init; }
    }

    private sealed class PlayerRow
    {
        public uint EntityId { get; init; }
        public float PositionX { get; init; }
        public float PositionY { get; init; }
        public float PositionZ { get; init; }
        public float RotationYaw { get; init; }
        public string LastSeenUtc { get; init; } = string.Empty;
        public int Coins { get; init; } = EconomyConfig.StartingCoins;
        public int VillageReputation { get; init; }
        public float Energy { get; init; } = 100f;
        public float Hunger { get; init; } = PlayerHungerConfig.DefaultHunger;
        public float Mood { get; init; } = PlayerNeedsConfig.DefaultMood;
        public float Fatigue { get; init; } = PlayerNeedsConfig.DefaultFatigue;
        public float SocialNeed { get; init; } = PlayerNeedsConfig.DefaultSocialNeed;
        public long NeedsLastGameMinute { get; init; }
        public int VillageContributionScore { get; init; }
        public int VillageTitleId { get; init; } = (int)VillageTitle.Newcomer;
        public int VillagePositionId { get; init; }
        public string? PositionAssignedAtUtc { get; init; }

        public PlayerRecord ToRecord()
        {
            return new PlayerRecord
            {
                EntityId = EntityId,
                PositionX = PositionX,
                PositionY = PositionY,
                PositionZ = PositionZ,
                RotationYaw = RotationYaw,
                LastSeenUtc = DateTime.Parse(LastSeenUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
                Coins = Coins,
                VillageReputation = VillageReputation,
                Energy = Energy,
                Hunger = Hunger,
                Mood = Mood,
                Fatigue = Fatigue,
                SocialNeed = SocialNeed,
                NeedsLastGameMinute = NeedsLastGameMinute,
                VillageContributionScore = VillageContributionScore,
                VillageTitle = (VillageTitle)VillageTitleId,
                VillagePosition = (VillagePosition)VillagePositionId,
                PositionAssignedAtUtc = ParseOptionalUtc(PositionAssignedAtUtc),
            };
        }
    }
}