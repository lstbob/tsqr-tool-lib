using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain;

public interface IManufacturerRepository
{
    Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Manufacturer?> GetByIdAsync(ManufcaturerId id, CancellationToken cancellationToken = default);
}
