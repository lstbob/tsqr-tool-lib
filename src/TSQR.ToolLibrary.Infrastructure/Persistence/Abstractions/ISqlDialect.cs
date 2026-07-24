namespace TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;

public interface ISqlDialect
{
    string SelectInsertedIdSuffix { get; }

    bool IsTransient(Exception ex);
}
