namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Persistence contract for document/NoSQL stores. These stores address
/// documents by an opaque string key and expose asynchronous writes rather
/// than a relational unit of work.
/// </summary>
public interface INoSqlRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    Task<TAggregateRoot?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TAggregateRoot entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
