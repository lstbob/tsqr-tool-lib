using System.Data;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

public class DapperUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly SqlConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public DapperUnitOfWork(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
    }

    public SqlConnection Connection => _connection;
    public IDbTransaction? Transaction => _transaction;

    public void EnsureOpen()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
    }

    public void BeginTransaction()
    {
        EnsureOpen();
        if (_transaction is null)
            _transaction = _connection.BeginTransaction();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
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
