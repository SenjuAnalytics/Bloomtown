using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists yes/no votes cast on village project proposals.
/// </summary>
public sealed class ProjectVoteRepository
{
    private readonly string _connectionString;

    public ProjectVoteRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InsertAsync(ProjectVoteRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO project_votes (proposal_id, player_entity_id, vote, vote_weight, voted_at)
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
            FROM project_votes
            WHERE proposal_id = @ProposalId AND player_entity_id = @PlayerEntityId
            """,
            new { ProposalId = proposalId, PlayerEntityId = playerEntityId });

        return count > 0;
    }

    public async Task<IReadOnlyList<ProjectVoteRecord>> GetByProposalAsync(int proposalId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                proposal_id AS ProposalId,
                player_entity_id AS PlayerEntityId,
                vote AS Vote,
                vote_weight AS VoteWeight,
                voted_at AS VotedAtUtc
            FROM project_votes
            WHERE proposal_id = @ProposalId
            ORDER BY voted_at ASC
            """;

        var rows = await connection.QueryAsync<VoteRow>(sql, new { ProposalId = proposalId });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    /// <summary>
    /// Tallies both voter counts and weighted vote totals for a proposal.
    /// </summary>
    public async Task<WeightedVoteTally> GetWeightedTallyAsync(int proposalId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                vote AS Vote,
                COUNT(1) AS VoteCount,
                COALESCE(SUM(vote_weight), 0) AS VoteWeight
            FROM project_votes
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

    private sealed class VoteRow
    {
        public int ProposalId { get; init; }
        public uint PlayerEntityId { get; init; }
        public int Vote { get; init; }
        public int VoteWeight { get; init; }
        public string VotedAtUtc { get; init; } = string.Empty;

        public ProjectVoteRecord ToRecord()
        {
            return new ProjectVoteRecord
            {
                ProposalId = ProposalId,
                PlayerEntityId = PlayerEntityId,
                Vote = (ProjectVoteChoice)Vote,
                VoteWeight = VoteWeight,
                VotedAtUtc = DateTime.Parse(VotedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}