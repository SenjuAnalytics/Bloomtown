using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists yes/no votes cast by Village Council members on important proposals.
/// </summary>
public sealed class CouncilProposalVoteRepository
{
    private readonly string _connectionString;

    public CouncilProposalVoteRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InsertAsync(CouncilProposalVoteRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO council_proposal_votes (proposal_id, player_entity_id, vote, vote_weight, voted_at)
            VALUES (@ProposalId, @PlayerEntityId, @Vote, @VoteWeight, @VotedAtUtc)
            """,
            new
            {
                record.ProposalId,
                record.PlayerEntityId,
                Vote = (int)record.Vote,
                record.VoteWeight,
                VotedAtUtc = record.VotedAtUtc.ToString("O"),
            });
    }

    public async Task<bool> HasVotedAsync(int proposalId, uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1)
            FROM council_proposal_votes
            WHERE proposal_id = @ProposalId AND player_entity_id = @PlayerEntityId
            """,
            new { ProposalId = proposalId, PlayerEntityId = playerEntityId });

        return count > 0;
    }

    public async Task<WeightedVoteTally> GetWeightedTallyAsync(int proposalId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                vote AS Vote,
                COUNT(1) AS VoteCount,
                COALESCE(SUM(vote_weight), 0) AS VoteWeight
            FROM council_proposal_votes
            WHERE proposal_id = @ProposalId
            GROUP BY vote
            """;

        var rows = await connection.QueryAsync<(int Vote, int VoteCount, int VoteWeight)>(
            sql,
            new { ProposalId = proposalId });

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