using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

public class SqlRepository<TEntity, TId>(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<TEntity> mapping,
    ISqlDialect dialect)
    : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : ValueObject
{
    public IUnitOfWork UnitOfWork => uow;
    protected ISqlConnection Database => uow.Connection;

    public virtual async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return await mapping.GetByIdAsync(uow.Connection, (int)((dynamic)id!).Value);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var db = uow.Connection;
        var sql = mapping.InsertSql + dialect.SelectInsertedIdSuffix;
        var id = await db.ExecuteScalarAsync<int>(sql, mapping.ToInsertParameters(entity));

        entity.SetAssignedId((TId)Activator.CreateInstance(typeof(TId), [id])!);
    }

    public virtual void Update(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.UpdateSql, mapping.ToUpdateParameters(entity));
    }

    public virtual void Delete(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.DeleteSql, new { Id = entity.Id });
    }
}
