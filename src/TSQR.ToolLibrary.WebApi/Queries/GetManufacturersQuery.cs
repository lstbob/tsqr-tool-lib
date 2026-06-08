using MediatR;
using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetManufacturersQuery : IRequest<List<ManufacturerDto>>;

public sealed class GetManufacturersHandler : IRequestHandler<GetManufacturersQuery, List<ManufacturerDto>>
{
    private readonly IManufacturerRepository _manufacturerRepo;

    public GetManufacturersHandler(IManufacturerRepository manufacturerRepo)
    {
        _manufacturerRepo = manufacturerRepo;
    }

    public async Task<List<ManufacturerDto>> Handle(GetManufacturersQuery request, CancellationToken ct)
    {
        var manufacturers = await _manufacturerRepo.GetAllAsync(ct);
        return manufacturers.Select(m => new ManufacturerDto(m.Id.Value, m.Name)).ToList();
    }
}
