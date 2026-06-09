using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Route("api/manufacturers")]
public sealed class ManufacturersController : ControllerBase
{
    private readonly IInteractor<GetManufacturersQuery, List<ManufacturerDto>> _getManufacturers;

    public ManufacturersController(IInteractor<GetManufacturersQuery, List<ManufacturerDto>> getManufacturers)
    {
        _getManufacturers = getManufacturers;
    }

    [HttpGet]
    public async Task<List<ManufacturerDto>> GetAll()
    {
        return await _getManufacturers.ExecuteAsync(new GetManufacturersQuery());
    }
}
