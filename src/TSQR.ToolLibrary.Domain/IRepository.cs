namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Technology-agnostic persistence contract for an aggregate root.
/// Backend-specific contracts (relational, document, ...) extend this
/// interface with their own unit-of-work or session semantics.
/// </summary>
public interface IRepository<TAggregateRoot, TId>
where TAggregateRoot : IAggregateRoot
{
    Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default);

    void Update(TAggregateRoot entity);

    void Delete(TAggregateRoot entity);
}
