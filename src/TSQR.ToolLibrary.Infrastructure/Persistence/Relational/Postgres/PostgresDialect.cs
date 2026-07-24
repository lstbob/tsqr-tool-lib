using System.Net.Sockets;
using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Postgres;

public sealed class PostgresDialect : ISqlDialect
{
    public string SelectInsertedIdSuffix => "\nRETURNING Id";

    public bool IsTransient(Exception ex)
    {
        if (ex is NpgsqlException npgsql)
            return npgsql.IsTransient;
        if (ex is IOException)
            return true;
        if (ex is SocketException)
            return true;
        return false;
    }
}
