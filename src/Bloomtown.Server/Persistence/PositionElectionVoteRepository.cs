using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists weighted votes cast in village leadership elections.
/// </summary>
public sealed class PositionElectionVoteRepository
{
    private readonly string _connectionString;

    public PositionElectionVoteRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InsertAsync(PositionElectionVoteRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO village_position_election_votes (
                election_id, player_entity_id, vote, vote_weight, voted_at
            )
            VALUES (
                @ElectionId, @PlayerEntityId, @Vote, @VoteWeight, @VotedAtUtc
            )
            """,
            new
            {
                record.ElectionId,
                record.PlayerEntityId,
                Vote = (int)record.Vote,
                record.VoteWeight,
                VotedAtUtc = record.VotedAtUtc.ToString("O"),
            });
    }

    public async Task<bool> HasVotedAsync(int electionId, uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1)
            FROM village_position_election_votes
            WHERE election_id = @ElectionId AND player_entity_id = @PlayerEntityId
            """,
            new { ElectionId = electionId, PlayerEntityId = playerEntityId });

        return count > 0;
    }

    public async Task<WeightedVoteTally> GetWeightedTallyAsync(int electionId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                vote AS Vote,
                COUNT(1) AS VoteCount,
                COALESCE(SUM(vote_weight), 0) AS VoteWeight
            FROM village_position_election_votes
            WHERE election_id = @ElectionId
            GROUP BY vote
            """;

        var rows = await connection.QueryAsync<(int Vote, int VoteCount, int VoteWeight)>(
            sql,
            new { ElectionId = electionId });

        var yesCount = 0;
        var noCount = 0;
        var yesWeight = 0;
        var noWeight = 0;

        foreach (var (vote, voteCount, voteWeight) in rows)
        {
            if (vote == (int)ProjectVoteChoice.Yes)
            {
                yesCount = voteCount;
                yesWeight = voteWeight;
            }
            else if (vote == (int)ProjectVoteChoice.No)
            {
                noCount = voteCount;
                noWeight = voteWeight;
            }
        }

        return new WeightedVoteTally(yesCount, noCount, yesWeight, noWeight);
    }
}