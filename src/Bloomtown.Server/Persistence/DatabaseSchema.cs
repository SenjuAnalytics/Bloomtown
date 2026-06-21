namespace Bloomtown.Server.Persistence;

/// <summary>
/// SQLite schema for Spike 5 persistence (players, NPCs, world time).
/// </summary>
public static class DatabaseSchema
{
    public const string CreateTablesSql = """
        CREATE TABLE IF NOT EXISTS players (
            entity_id INTEGER PRIMARY KEY,
            position_x REAL NOT NULL,
            position_y REAL NOT NULL,
            position_z REAL NOT NULL,
            rotation_yaw REAL NOT NULL,
            last_seen TEXT NOT NULL,
            coins INTEGER NOT NULL DEFAULT 100
        );

        CREATE TABLE IF NOT EXISTS player_inventory (
            player_entity_id INTEGER NOT NULL,
            item_type INTEGER NOT NULL,
            quantity INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (player_entity_id, item_type)
        );

        CREATE TABLE IF NOT EXISTS npcs (
            entity_id INTEGER PRIMARY KEY,
            name TEXT NOT NULL,
            position_x REAL NOT NULL,
            position_y REAL NOT NULL,
            position_z REAL NOT NULL,
            hunger REAL NOT NULL DEFAULT 35,
            energy REAL NOT NULL DEFAULT 80,
            social REAL NOT NULL DEFAULT 30
        );

        CREATE TABLE IF NOT EXISTS world_state (
            id INTEGER PRIMARY KEY CHECK (id = 1),
            game_day INTEGER NOT NULL,
            game_minute INTEGER NOT NULL,
            last_saved TEXT NOT NULL,
            village_development_level INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS player_npc_reputation (
            player_entity_id INTEGER NOT NULL,
            npc_entity_id INTEGER NOT NULL,
            reputation_value INTEGER NOT NULL DEFAULT 0,
            last_updated TEXT NOT NULL,
            PRIMARY KEY (player_entity_id, npc_entity_id)
        );

        CREATE TABLE IF NOT EXISTS player_npc_relationship (
            player_entity_id INTEGER NOT NULL,
            npc_entity_id INTEGER NOT NULL,
            affinity_value INTEGER NOT NULL DEFAULT 0,
            last_interaction_game_day INTEGER NOT NULL DEFAULT 0,
            last_interaction TEXT NOT NULL,
            PRIMARY KEY (player_entity_id, npc_entity_id)
        );

        CREATE TABLE IF NOT EXISTS player_npc_memories (
            player_entity_id INTEGER NOT NULL,
            npc_entity_id INTEGER NOT NULL,
            memory_type INTEGER NOT NULL,
            occurred_at TEXT NOT NULL,
            PRIMARY KEY (player_entity_id, npc_entity_id, memory_type)
        );

        CREATE TABLE IF NOT EXISTS player_chest (
            player_entity_id INTEGER NOT NULL,
            item_type INTEGER NOT NULL,
            quantity INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (player_entity_id, item_type)
        );

        CREATE TABLE IF NOT EXISTS player_housing (
            player_entity_id INTEGER PRIMARY KEY,
            house_x REAL NOT NULL,
            house_z REAL NOT NULL,
            house_tier INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS player_house_furniture (
            player_entity_id INTEGER NOT NULL,
            furniture_type INTEGER NOT NULL,
            quantity INTEGER NOT NULL DEFAULT 1,
            PRIMARY KEY (player_entity_id, furniture_type)
        );

        CREATE TABLE IF NOT EXISTS player_home_storage (
            player_entity_id INTEGER NOT NULL,
            item_type INTEGER NOT NULL,
            quantity INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (player_entity_id, item_type)
        );

        CREATE TABLE IF NOT EXISTS community_project_status (
            project_id INTEGER PRIMARY KEY,
            status INTEGER NOT NULL DEFAULT 0,
            completed_at TEXT
        );

        CREATE TABLE IF NOT EXISTS community_project_progress (
            project_id INTEGER NOT NULL,
            item_type INTEGER NOT NULL,
            current_quantity INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (project_id, item_type)
        );

        CREATE TABLE IF NOT EXISTS community_project_contributors (
            project_id INTEGER NOT NULL,
            player_entity_id INTEGER NOT NULL,
            total_contributed INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (project_id, player_entity_id)
        );

        CREATE TABLE IF NOT EXISTS village_milestones (
            milestone_id INTEGER PRIMARY KEY,
            unlocked_at TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS village_project_proposals (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            proposed_by_player_id INTEGER NOT NULL,
            project_name TEXT NOT NULL,
            project_slug TEXT NOT NULL,
            required_resources_json TEXT NOT NULL,
            status INTEGER NOT NULL DEFAULT 1,
            created_project_id INTEGER,
            created_at TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS community_project_definitions (
            project_id INTEGER PRIMARY KEY,
            slug TEXT NOT NULL UNIQUE,
            name TEXT NOT NULL,
            description TEXT NOT NULL,
            requirements_json TEXT NOT NULL,
            is_builtin INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS project_votes (
            proposal_id INTEGER NOT NULL,
            player_entity_id INTEGER NOT NULL,
            vote INTEGER NOT NULL,
            vote_weight INTEGER NOT NULL DEFAULT 1,
            voted_at TEXT NOT NULL,
            PRIMARY KEY (proposal_id, player_entity_id)
        );

        CREATE TABLE IF NOT EXISTS village_position_elections (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            position INTEGER NOT NULL,
            candidate_player_id INTEGER NOT NULL,
            status INTEGER NOT NULL DEFAULT 1,
            voting_end_total_minutes INTEGER NOT NULL,
            created_at TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS village_position_election_votes (
            election_id INTEGER NOT NULL,
            player_entity_id INTEGER NOT NULL,
            vote INTEGER NOT NULL,
            vote_weight INTEGER NOT NULL DEFAULT 1,
            voted_at TEXT NOT NULL,
            PRIMARY KEY (election_id, player_entity_id)
        );

        CREATE TABLE IF NOT EXISTS council_proposal_votes (
            proposal_id INTEGER NOT NULL,
            player_entity_id INTEGER NOT NULL,
            vote INTEGER NOT NULL,
            vote_weight INTEGER NOT NULL DEFAULT 1,
            voted_at TEXT NOT NULL,
            PRIMARY KEY (proposal_id, player_entity_id)
        );

        CREATE TABLE IF NOT EXISTS chief_authority_log (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            chief_player_id INTEGER NOT NULL,
            action_type INTEGER NOT NULL,
            proposal_id INTEGER NOT NULL,
            game_day INTEGER NOT NULL,
            created_at TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS unlocked_village_areas (
            area_id INTEGER PRIMARY KEY,
            unlocked_at TEXT NOT NULL
        );
        """;
}