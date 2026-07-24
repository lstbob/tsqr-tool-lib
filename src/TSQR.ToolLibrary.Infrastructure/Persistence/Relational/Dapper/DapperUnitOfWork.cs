using System.Data;
using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

public sealed class DapperUnitOfWork(
    IDbConnectionFactory connectionFactory,
    ISqlDialect dialect) : ISqlUnitOfWork, IDisposable
{
    private IDbConnection _connection = null!;
    private IDbTransaction? _transaction;
    private DapperConnection? _connectionAdapter;
    private bool _disposed;

    public ISqlConnection Connection
    {
        get
        {
            EnsureConnection();
            return _connectionAdapter!;
        }
    }

    private void EnsureConnection()
    {
        if (_connectionAdapter is not null)
            return;

        _connection = connectionFactory.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
        _connectionAdapter = new DapperConnection(_connection, _transaction);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;
        const int delayMs = 100;

        for (int attempt = 1; ; attempt++)
        {
            if (_transaction is null)
                EnsureConnection();

            try
            {
                _transaction!.Commit();
                _transaction.Dispose();
                _transaction = null;
                _connectionAdapter = new DapperConnection(_connection, null);
                return 0;
            }
            catch (Exception ex) when (attempt < maxAttempts && dialect.IsTransient(ex))
            {
                try { _transaction?.Rollback(); } catch { }
                _transaction?.Dispose();
                _transaction = null;
                _connectionAdapter = null;
                await Task.Delay(delayMs * attempt, cancellationToken);
            }
        }
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
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
