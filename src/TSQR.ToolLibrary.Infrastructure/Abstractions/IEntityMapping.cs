namespace TSQR.ToolLibrary.Infrastructure.Abstractions;

public interface IEntityMapping<TEntity>
{
    string TableName { get; }

    string InsertSql { get; }
    string UpdateSql { get; }
    string DeleteSql { get; }

    Task<TEntity?> GetByIdAsync(IDatabaseConnection db, object id);
    object ToInsertParameters(TEntity entity);
    object ToUpdateParameters(TEntity entity);
}
