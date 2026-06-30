namespace TSQR.ToolLibrary.Domain;

public interface IManufacturerRepository
{
    Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Manufacturer?> GetByIdAsync(
        ManufacturerId id,
        CancellationToken cancellationToken = default
    );
}
