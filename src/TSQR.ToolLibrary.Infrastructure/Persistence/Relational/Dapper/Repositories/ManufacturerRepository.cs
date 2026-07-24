using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class ManufacturerRepository(ISqlUnitOfWork uow) : IManufacturerRepository
{
    public async Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await uow.Connection.QueryAsync<ManufacturerRow>(
            "SELECT Id, Name FROM Manufacturers ORDER BY Name");

        return rows.Select(r => Manufacturer.Create(new ManufacturerId(r.Id), r.Name)).ToList();
    }

    public async Task<Manufacturer?> GetByIdAsync(ManufacturerId id, CancellationToken cancellationToken = default)
    {
        var row = await uow.Connection.QuerySingleOrDefaultAsync<ManufacturerRow>(
            "SELECT Id, Name FROM Manufacturers WHERE Id = @Id", new { Id = id.Value });

        return row is null ? null : Manufacturer.Create(new ManufacturerId(row.Id), row.Name);
    }

    private sealed record ManufacturerRow(int Id, string Name);
}
