using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

public sealed class ManufacturerRepository : IManufacturerRepository
{
    private readonly IDatabaseUnitOfWork _uow;

    public ManufacturerRepository(IDatabaseUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _uow.Connection.QueryAsync<ManufacturerRow>(
            "SELECT Id, Name FROM Manufacturers ORDER BY Name");

        return rows.Select(r => Manufacturer.Create(new ManufcaturerId(r.Id), r.Name)).ToList();
    }

    private sealed record ManufacturerRow(int Id, string Name);
}
