namespace TSQR.ToolLibrary.Domain;

public interface IRepository<TAggregateRoot, TId>  
where TAggregateRoot : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}

