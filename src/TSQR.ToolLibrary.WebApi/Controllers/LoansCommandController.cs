using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Route("api/loans")]
public sealed class LoansCommandController : ControllerBase
{
    private readonly IInteractor<MarkLoanAsNotReturnedCommand, Result> _markNotReturned;

    public LoansCommandController(IInteractor<MarkLoanAsNotReturnedCommand, Result> markNotReturned)
    {
        _markNotReturned = markNotReturned;
    }

    [HttpPost("mark-not-returned")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkNotReturned(MarkLoanAsNotReturnedRequest request, CancellationToken ct)
    {
        var command = new MarkLoanAsNotReturnedCommand(
            new LoanId(request.LoanId));

        var result = await _markNotReturned.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    private ActionResult ToErrorResult(Error error)
    {
        return error switch
        {
            NotFoundError err => NotFound(new ErrorResponse(err.Code, err.Message)),
            _ => BadRequest(new ErrorResponse(error.Code, error.Message)),
        };
    }
}
