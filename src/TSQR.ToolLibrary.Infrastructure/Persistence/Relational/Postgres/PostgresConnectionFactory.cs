using System.Data;
using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Postgres;

public sealed class PostgresConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
