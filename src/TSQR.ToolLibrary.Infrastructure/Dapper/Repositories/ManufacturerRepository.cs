using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

public sealed class ManufacturerRepository : IManufacturerRepository
{
    private readonly ISqlUnitOfWork _uow;

    public ManufacturerRepository(ISqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _uow.Connection.QueryAsync<ManufacturerRow>(
            "SELECT Id, Name FROM Manufacturers ORDER BY Name");

        return rows.Select(r => Manufacturer.Create(new ManufacturerId(r.Id), r.Name)).ToList();
    }

    public async Task<Manufacturer?> GetByIdAsync(ManufacturerId id, CancellationToken cancellationToken = default)
    {
        var row = await _uow.Connection.QuerySingleOrDefaultAsync<ManufacturerRow>(
            "SELECT Id, Name FROM Manufacturers WHERE Id = @Id", new { Id = id.Value });

        return row is null ? null : Manufacturer.Create(new ManufacturerId(row.Id), row.Name);
    }

    private sealed record ManufacturerRow(int Id, string Name);
}
