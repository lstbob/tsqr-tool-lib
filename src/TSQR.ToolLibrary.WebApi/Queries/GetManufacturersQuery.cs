using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetManufacturersQuery;

public sealed class GetManufacturersHandler : IInteractor<GetManufacturersQuery, List<ManufacturerDto>>
{
    private readonly IManufacturerRepository _manufacturerRepo;

    public GetManufacturersHandler(IManufacturerRepository manufacturerRepo)
    {
        _manufacturerRepo = manufacturerRepo;
    }

    public async Task<List<ManufacturerDto>> ExecuteAsync(GetManufacturersQuery request, CancellationToken ct)
    {
        var manufacturers = await _manufacturerRepo.GetAllAsync(ct);
        return manufacturers.Select(m => new ManufacturerDto(m.Id.Value, m.Name)).ToList();
    }
}
