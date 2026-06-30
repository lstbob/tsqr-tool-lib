namespace TSQR.ToolLibrary.Infrastructure.Dapper;

/// <summary>
/// SQL-specific connection abstraction used by Dapper repositories and entity
/// mappings. This interface is intentionally Dapper/SQL-shaped: it lives in the
/// <c>Infrastructure.Dapper</c> namespace and is only consumed by SQL
/// repositories. The application layer never sees it - it depends on the
/// technology-agnostic <c>IRepository&lt;T&gt;</c> / <c>IUnitOfWork</c> contracts
/// from the Domain layer. A NoSQL or document adapter would expose a different
/// connection contract under its own subfolder.
/// </summary>
public interface ISqlConnection : IDisposable
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null);
    Task<int> ExecuteAsync(string sql, object? parameters = null);

    IEnumerable<T> Query<T>(string sql, object? parameters = null);
    T? QuerySingleOrDefault<T>(string sql, object? parameters = null);
    T ExecuteScalar<T>(string sql, object? parameters = null);
    int Execute(string sql, object? parameters = null);
}