namespace TSQR.ToolLibrary.Domain;

public interface IRepository<TAggregateRoot, TId>  
where TAggregateRoot : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }

    Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default);

    void Update(TAggregateRoot entity);

    void Delete(TAggregateRoot entity);
}

