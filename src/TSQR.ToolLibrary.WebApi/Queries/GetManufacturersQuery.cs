using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetManufacturersQuery;

public sealed class GetManufacturersHandler(IManufacturerRepository manufacturerRepo)
    : IInteractor<GetManufacturersQuery, List<ManufacturerDto>>
{
    public async Task<List<ManufacturerDto>> ExecuteAsync(
        GetManufacturersQuery request,
        CancellationToken ct
    )
    {
        var manufacturers = await manufacturerRepo.GetAllAsync(ct);
        return manufacturers.Select(m => new ManufacturerDto(m.Id.Value, m.Name)).ToList();
    }
}
