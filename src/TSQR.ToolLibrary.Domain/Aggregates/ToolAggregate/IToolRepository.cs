namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Repository for the <see cref="Tool"/> aggregate. Read-model queries live in
/// the application layer instead of the domain.
/// </summary>
public interface IToolRepository : IRepository<Tool, ToolId>
{
}
