using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace PulseQueue.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 30;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
                await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

                await connection.ExecuteAsync(new CommandDefinition(
                    CreateJobsTableSql,
                    cancellationToken: cancellationToken));

                return;
            }
            catch when (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    private const string CreateJobsTableSql = """
        create table if not exists jobs (
            id uuid primary key,
            type varchar(100) not null,
            payload jsonb not null,
            status varchar(32) not null,
            created_at timestamptz not null,
            updated_at timestamptz not null,
            started_at timestamptz null,
            completed_at timestamptz null,
            failure_reason varchar(2000) null
        );

        create index if not exists ix_jobs_status on jobs (status);
        create index if not exists ix_jobs_created_at on jobs (created_at);
        """;
}
