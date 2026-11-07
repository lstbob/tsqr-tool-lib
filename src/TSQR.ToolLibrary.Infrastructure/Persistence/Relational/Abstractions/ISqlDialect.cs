namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

public interface ISqlDialect
{
    string SelectInsertedIdSuffix { get; }

    bool IsTransient(Exception ex);
}
