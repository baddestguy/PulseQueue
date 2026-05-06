using Dapper;
using PulseQueue.Domain;
using PulseQueue.Infrastructure.Data;

namespace PulseQueue.Infrastructure.Jobs;

public sealed class JobRepository : IJobRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public JobRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(Job job, CancellationToken cancellationToken)
    {
        const string sql = """
            insert into jobs (
                id,
                type,
                payload,
                status,
                created_at,
                updated_at,
                started_at,
                completed_at,
                failure_reason
            )
            values (
                @Id,
                @Type,
                cast(@Payload as jsonb),
                @Status,
                @CreatedAt,
                @UpdatedAt,
                @StartedAt,
                @CompletedAt,
                @FailureReason
            );
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            ToDatabaseParameters(job),
            cancellationToken: cancellationToken));
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                id as Id,
                type as Type,
                payload::text as Payload,
                status as Status,
                created_at as CreatedAt,
                updated_at as UpdatedAt,
                started_at as StartedAt,
                completed_at as CompletedAt,
                failure_reason as FailureReason
            from jobs
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Job>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task MarkProcessingAsync(Guid id, DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        const string sql = """
            update jobs
            set
                status = @Status,
                started_at = @StartedAt,
                updated_at = @StartedAt
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                Status = JobStatus.Processing.ToString(),
                StartedAt = startedAt
            },
            cancellationToken: cancellationToken));
    }

    public async Task MarkSucceededAsync(Guid id, DateTimeOffset completedAt, CancellationToken cancellationToken)
    {
        const string sql = """
            update jobs
            set
                status = @Status,
                completed_at = @CompletedAt,
                updated_at = @CompletedAt
            where id = @Id;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id,
                Status = JobStatus.Succeeded.ToString(),
                CompletedAt = completedAt
            },
            cancellationToken: cancellationToken));
    }

    private static object ToDatabaseParameters(Job job) =>
        new
        {
            job.Id,
            job.Type,
            job.Payload,
            Status = job.Status.ToString(),
            job.CreatedAt,
            job.UpdatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.FailureReason
        };
}
