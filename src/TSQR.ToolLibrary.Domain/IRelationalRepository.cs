namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Relational repository contract. The unit of work exposes the transaction
/// that spans multiple repository operations.
/// </summary>
public interface IRelationalRepository<TAggregateRoot, TId>
    : IRepository<TAggregateRoot, TId>
    where TAggregateRoot : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
