using TSQR.ToolLibrary.Infrastructure.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

public sealed class DapperUnitOfWork : IDatabaseUnitOfWork, IDisposable
{
    private readonly NpgsqlConnection _connection;
    private IDbTransaction? _transaction;
    private DapperConnection? _connectionAdapter;
    private bool _disposed;

    public DapperUnitOfWork(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    public IDatabaseConnection Connection
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
            _connectionAdapter = new DapperConnection(_connection, null);
        }
        return Task.FromResult(0);
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
