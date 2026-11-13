namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Read-only access to the manufacturer catalog. Manufacturers are value
/// objects referenced by tools; they are not aggregate roots.
/// </summary>
public interface IManufacturerLookup
{
    Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Manufacturer?> GetByIdAsync(
        ManufacturerId id,
        CancellationToken cancellationToken = default
    );
}
