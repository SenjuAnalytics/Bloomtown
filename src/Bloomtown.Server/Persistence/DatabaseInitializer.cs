using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public static class DatabaseInitializer
{
    public static void Initialize(PersistenceOptions options)
    {
        var directory = Path.GetDirectoryName(options.DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        using var connection = CreateConnection(options.DatabasePath);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = DatabaseSchema.CreateTablesSql;
        command.ExecuteNonQuery();

        MigrateNpcNeedsColumns(connection);
        MigratePlayerEconomyColumns(connection);
        MigrateProposalVotingColumns(connection);
        MigrateProjectVoteWeightColumn(connection);
        MigrateCouncilAuthorityTables(connection);
        MigratePlayerHousingTables(connection);
        MigratePlayerNpcRelationshipTable(connection);
        MigratePlayerHouseFurnitureTable(connection);
        MigratePlayerNeedsColumns(connection);
        MigratePlayerNpcMemoriesTable(connection);
        MigrateCommunityProjectCompletedAtColumn(connection);
        MigrateWorldStateVillageDevelopmentLevelColumn(connection);
        MigrateUnlockedVillageAreasTable(connection);
        MigratePlayerCommunityReputationTable(connection);
        MigratePlayerLongTermGoalTable(connection);
        MigratePlayerPersonalMilestoneTable(connection);
    }

    private static void MigratePlayerPersonalMilestoneTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS player_personal_milestone (
                player_entity_id INTEGER PRIMARY KEY,
                completed_bitmask INTEGER NOT NULL DEFAULT 0,
                rhythm_agency_days TEXT NOT NULL DEFAULT '',
                daily_activity_count INTEGER NOT NULL DEFAULT 0,
                daily_activity_days TEXT NOT NULL DEFAULT ''
            )
            """;
        create.ExecuteNonQuery();
    }

    private static void MigratePlayerLongTermGoalTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS player_long_term_goal (
                player_entity_id INTEGER PRIMARY KEY,
                goal_kind INTEGER NOT NULL DEFAULT 1,
                highest_completed_milestone INTEGER NOT NULL DEFAULT 0,
                goal_completed_at TEXT
            )
            """;
        create.ExecuteNonQuery();
        MigratePlayerLongTermGoalLegacyArchetypeColumn(connection);
        MigratePlayerLongTermGoalInfluenceColumns(connection);
        MigratePlayerLongTermGoalLegacyFocusColumn(connection);
    }

    private static void MigratePlayerLongTermGoalLegacyFocusColumn(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(player_long_term_goal)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("legacy_focus"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE player_long_term_goal ADD COLUMN legacy_focus INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerLongTermGoalInfluenceColumns(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(player_long_term_goal)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("builder_influence"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE player_long_term_goal ADD COLUMN builder_influence INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("caretaker_influence"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE player_long_term_goal ADD COLUMN caretaker_influence INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("connector_influence"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE player_long_term_goal ADD COLUMN connector_influence INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerLongTermGoalLegacyArchetypeColumn(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(player_long_term_goal)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("legacy_archetype"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE player_long_term_goal ADD COLUMN legacy_archetype INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerCommunityReputationTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS player_community_reputation (
                player_entity_id INTEGER PRIMARY KEY,
                help_garden_count INTEGER NOT NULL DEFAULT 0,
                help_market_count INTEGER NOT NULL DEFAULT 0,
                help_well_count INTEGER NOT NULL DEFAULT 0
            )
            """;
        create.ExecuteNonQuery();
    }

    private static void MigrateUnlockedVillageAreasTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS unlocked_village_areas (
                area_id INTEGER PRIMARY KEY,
                unlocked_at TEXT NOT NULL
            )
            """;
        create.ExecuteNonQuery();
    }

    private static void MigrateWorldStateVillageDevelopmentLevelColumn(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(world_state)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("village_development_level"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText =
                "ALTER TABLE world_state ADD COLUMN village_development_level INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigrateCommunityProjectCompletedAtColumn(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(community_project_status)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("completed_at"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE community_project_status ADD COLUMN completed_at TEXT";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigrateProjectVoteWeightColumn(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(project_votes)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("vote_weight"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE project_votes ADD COLUMN vote_weight INTEGER NOT NULL DEFAULT 1";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigrateProposalVotingColumns(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(village_project_proposals)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("voting_end_total_minutes"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE village_project_proposals ADD COLUMN voting_end_total_minutes INTEGER";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerEconomyColumns(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(players)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("coins"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN coins INTEGER NOT NULL DEFAULT 100";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("village_reputation"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN village_reputation INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("energy"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN energy REAL NOT NULL DEFAULT 100";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("village_contribution_score"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN village_contribution_score INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("village_title"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN village_title INTEGER NOT NULL DEFAULT 1";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("village_position"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN village_position INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("position_assigned_at"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN position_assigned_at TEXT";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigrateCouncilAuthorityTables(SqliteConnection connection)
    {
        var proposalColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(village_project_proposals)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                proposalColumns.Add(reader.GetString(1));
        }

        if (!proposalColumns.Contains("project_tier"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE village_project_proposals ADD COLUMN project_tier INTEGER NOT NULL DEFAULT 1";
            alter.ExecuteNonQuery();
        }

        using (var createCouncilVotes = connection.CreateCommand())
        {
            createCouncilVotes.CommandText = """
                CREATE TABLE IF NOT EXISTS council_proposal_votes (
                    proposal_id INTEGER NOT NULL,
                    player_entity_id INTEGER NOT NULL,
                    vote INTEGER NOT NULL,
                    vote_weight INTEGER NOT NULL DEFAULT 1,
                    voted_at TEXT NOT NULL,
                    PRIMARY KEY (proposal_id, player_entity_id)
                )
                """;
            createCouncilVotes.ExecuteNonQuery();
        }

        using (var createChiefLog = connection.CreateCommand())
        {
            createChiefLog.CommandText = """
                CREATE TABLE IF NOT EXISTS chief_authority_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    chief_player_id INTEGER NOT NULL,
                    action_type INTEGER NOT NULL,
                    proposal_id INTEGER NOT NULL,
                    game_day INTEGER NOT NULL,
                    created_at TEXT NOT NULL
                )
                """;
            createChiefLog.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerHousingTables(SqliteConnection connection)
    {
        var playerColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(players)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                playerColumns.Add(reader.GetString(1));
        }

        if (!playerColumns.Contains("hunger"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN hunger REAL NOT NULL DEFAULT 20";
            alter.ExecuteNonQuery();
        }

        using (var createHousing = connection.CreateCommand())
        {
            createHousing.CommandText = """
                CREATE TABLE IF NOT EXISTS player_housing (
                    player_entity_id INTEGER PRIMARY KEY,
                    house_x REAL NOT NULL,
                    house_z REAL NOT NULL,
                    house_tier INTEGER NOT NULL DEFAULT 0
                )
                """;
            createHousing.ExecuteNonQuery();
        }

        var housingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(player_housing)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                housingColumns.Add(reader.GetString(1));
        }

        if (!housingColumns.Contains("house_tier"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE player_housing ADD COLUMN house_tier INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }

        using (var createStorage = connection.CreateCommand())
        {
            createStorage.CommandText = """
                CREATE TABLE IF NOT EXISTS player_home_storage (
                    player_entity_id INTEGER NOT NULL,
                    item_type INTEGER NOT NULL,
                    quantity INTEGER NOT NULL DEFAULT 0,
                    PRIMARY KEY (player_entity_id, item_type)
                )
                """;
            createStorage.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerHouseFurnitureTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS player_house_furniture (
                player_entity_id INTEGER NOT NULL,
                furniture_type INTEGER NOT NULL,
                quantity INTEGER NOT NULL DEFAULT 1,
                PRIMARY KEY (player_entity_id, furniture_type)
            )
            """;
        create.ExecuteNonQuery();
    }

    private static void MigratePlayerNpcRelationshipTable(SqliteConnection connection)
    {
        using (var create = connection.CreateCommand())
        {
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS player_npc_relationship (
                    player_entity_id INTEGER NOT NULL,
                    npc_entity_id INTEGER NOT NULL,
                    affinity_value INTEGER NOT NULL DEFAULT 0,
                    last_interaction_game_day INTEGER NOT NULL DEFAULT 0,
                    last_interaction TEXT NOT NULL,
                    PRIMARY KEY (player_entity_id, npc_entity_id)
                )
                """;
            create.ExecuteNonQuery();
        }

        using (var migrate = connection.CreateCommand())
        {
            migrate.CommandText = """
                INSERT OR IGNORE INTO player_npc_relationship (
                    player_entity_id,
                    npc_entity_id,
                    affinity_value,
                    last_interaction_game_day,
                    last_interaction)
                SELECT
                    player_entity_id,
                    npc_entity_id,
                    reputation_value,
                    0,
                    last_updated
                FROM player_npc_reputation
                """;
            migrate.ExecuteNonQuery();
        }
    }

    private static void MigrateNpcNeedsColumns(SqliteConnection connection)
    {
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(npcs)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                existingColumns.Add(reader.GetString(1));
        }

        if (!existingColumns.Contains("hunger"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE npcs ADD COLUMN hunger REAL NOT NULL DEFAULT 35";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("energy"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE npcs ADD COLUMN energy REAL NOT NULL DEFAULT 80";
            alter.ExecuteNonQuery();
        }

        if (!existingColumns.Contains("social"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE npcs ADD COLUMN social REAL NOT NULL DEFAULT 30";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerNeedsColumns(SqliteConnection connection)
    {
        var playerColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA table_info(players)";
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
                playerColumns.Add(reader.GetString(1));
        }

        if (!playerColumns.Contains("mood"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN mood REAL NOT NULL DEFAULT 70";
            alter.ExecuteNonQuery();
        }

        if (!playerColumns.Contains("fatigue"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN fatigue REAL NOT NULL DEFAULT 20";
            alter.ExecuteNonQuery();
        }

        if (!playerColumns.Contains("social_need"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN social_need REAL NOT NULL DEFAULT 25";
            alter.ExecuteNonQuery();
        }

        if (!playerColumns.Contains("needs_last_game_minute"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE players ADD COLUMN needs_last_game_minute INTEGER NOT NULL DEFAULT 0";
            alter.ExecuteNonQuery();
        }
    }

    private static void MigratePlayerNpcMemoriesTable(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
            CREATE TABLE IF NOT EXISTS player_npc_memories (
                player_entity_id INTEGER NOT NULL,
                npc_entity_id INTEGER NOT NULL,
                memory_type INTEGER NOT NULL,
                occurred_at TEXT NOT NULL,
                PRIMARY KEY (player_entity_id, npc_entity_id, memory_type)
            )
            """;
        create.ExecuteNonQuery();
    }

    public static SqliteConnection CreateConnection(string databasePath)
    {
        return new SqliteConnection($"Data Source={databasePath}");
    }
}