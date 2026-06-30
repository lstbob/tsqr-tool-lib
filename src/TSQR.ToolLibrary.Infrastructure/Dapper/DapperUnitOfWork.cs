using System.Net.Sockets;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

public sealed class DapperUnitOfWork(string connectionString) : ISqlUnitOfWork, IDisposable
{
    private readonly NpgsqlConnection _connection = new NpgsqlConnection(connectionString);
    private IDbTransaction? _transaction;
    private DapperConnection? _connectionAdapter;
    private bool _disposed;

    public ISqlConnection Connection
    {
        get
        {
            EnsureTransaction();
            return _connectionAdapter!;
        }
    }

    private void EnsureTransaction()
    {
        if (_transaction is not null) return;
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        _transaction = _connection.BeginTransaction();
        _connectionAdapter = new DapperConnection(_connection, _transaction);
    }

    /// <summary>
    /// Commits the current transaction with transient-failure retry. Npgsql
    /// <c>Commit</c> can fail transiently (network blip, connection drop);
    /// retrying here is the Npgsql equivalent of EF Core's
    /// <c>IExecutionStrategy</c>. This retry is technology-specific and lives
    /// in the Dapper adapter - never in the Domain or Application layer.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;
        const int delayMs = 100;

        for (int attempt = 1; ; attempt++)
        {
            if (_transaction is null)
                EnsureTransaction();

            try
            {
                _transaction!.Commit();
                _transaction.Dispose();
                _transaction = null;
                _connectionAdapter = new DapperConnection(_connection, null);
                return 0;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                try { _transaction?.Rollback(); } catch { }
                _transaction?.Dispose();
                _transaction = null;
                _connectionAdapter = new DapperConnection(_connection, null);
                await Task.Delay(delayMs * attempt, cancellationToken);
                EnsureTransaction();
            }
        }
    }

    /// <summary>
    /// Determines whether the given exception is likely transient (network /
    /// connection / timeout) and worth retrying. Mirrors the heuristics used
    /// by EF Core's <c>IExecutionStrategy</c> for Npgsql.
    /// </summary>
    private static bool IsTransient(Exception ex)
    {
        if (ex is NpgsqlException npgsql)
            return npgsql.IsTransient;
        if (ex is IOException)
            return true;
        if (ex is SocketException)
            return true;
        return false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_transaction is not null)
            {
                try { _transaction.Rollback(); } catch { }
                _transaction.Dispose();
            }
            _connection.Dispose();
            _disposed = true;
        }
    }
}
