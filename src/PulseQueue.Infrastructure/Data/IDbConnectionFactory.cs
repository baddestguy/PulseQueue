using System.Data.Common;

namespace PulseQueue.Infrastructure.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
