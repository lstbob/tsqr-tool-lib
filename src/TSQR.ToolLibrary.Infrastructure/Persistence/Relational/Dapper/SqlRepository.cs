using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

public class SqlRepository<TEntity, TId>(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<TEntity, TId> mapping,
    ISqlDialect dialect,
    IDomainEventCollector eventCollector)
    : IRelationalRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : ValueObject
{
    public IUnitOfWork UnitOfWork => uow;
    protected ISqlConnection Database => uow.Connection;

    public virtual Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return mapping.GetByIdAsync(uow.Connection, id);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var db = uow.Connection;
        var sql = mapping.InsertSql + dialect.SelectInsertedIdSuffix;
        var databaseId = await db.ExecuteScalarAsync<object>(
            sql,
            mapping.ToInsertParameters(entity));

        if (databaseId is null)
        {
            throw new InvalidOperationException(
                $"Insert for {typeof(TEntity).Name} did not return an identifier.");
        }

        entity.SetAssignedId(mapping.CreateId(databaseId));
        eventCollector.Track(entity);
    }

    public virtual void Update(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.UpdateSql, mapping.ToUpdateParameters(entity));
        eventCollector.Track(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.DeleteSql, mapping.ToDeleteParameters(entity));
    }
}
