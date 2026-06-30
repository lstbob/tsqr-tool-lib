using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[AllowAnonymous] // public read-only manufacturer list (consumed unauthenticated by the UI)
[Route("api/manufacturers")]
public sealed class ManufacturersController(
    IInteractor<GetManufacturersQuery, List<ManufacturerDto>> getManufacturers
) : ControllerBase
{
    [HttpGet]
    public async Task<List<ManufacturerDto>> GetAll()
    {
        return await getManufacturers.ExecuteAsync(new GetManufacturersQuery());
    }
}
