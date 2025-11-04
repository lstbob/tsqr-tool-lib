namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController("[controller]")]
public class LocationController(IMediatR mediator, ILogger<LocationController> logger)
    : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Ok)]
    [ProducesResponseType(StatusCodes.BadRequest)]
    [ProducesResponseType(StatusCodes.InternalServerError)]
    public Task<IActionResult> Post([Required] [FromBody] CreateLocationRequest request) { }
}
