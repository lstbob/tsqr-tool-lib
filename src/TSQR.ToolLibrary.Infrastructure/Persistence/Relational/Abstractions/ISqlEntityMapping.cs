using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

public interface ISqlEntityMapping<TEntity>
{
    string TableName { get; }

    string InsertSql { get; }
    string UpdateSql { get; }
    string DeleteSql { get; }

    Task<TEntity?> GetByIdAsync(ISqlConnection db, int id);
    object ToInsertParameters(TEntity entity);
    object ToUpdateParameters(TEntity entity);
}
