using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.Application.Inventory.Commands;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Application.Tool.Commands;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize] // state-changing tool operations require an authenticated caller
[Route("api/tools")]
public sealed class ToolsCommandController(
    IManufacturerRepository manufacturerRepository,
    IInteractor<RegisterToolCommand, Result<ToolId>> registerTool,
    IInteractor<UpdateToolDetailsCommand, Result> updateToolDetails,
    IInteractor<SetScarcityLevelCommand, Result> setScarcity,
    IInteractor<RemoveScarcityLevelCommand, Result> removeScarcity,
    IInteractor<LoanToolCommand, Result> loanTool,
    IInteractor<ReturnToolCommand, Result> returnTool,
    IInteractor<MarkToolForRepairCommand, Result> markForRepair,
    IInteractor<CompleteRepairCommand, Result> completeRepair,
    IInteractor<MarkToolAsLostCommand, Result> markAsLost
) : ControllerBase
{
    private readonly IManufacturerRepository _manufacturerRepository = manufacturerRepository;
    private readonly IInteractor<RegisterToolCommand, Result<ToolId>> _registerTool = registerTool;
    private readonly IInteractor<UpdateToolDetailsCommand, Result> _updateToolDetails =
        updateToolDetails;
    private readonly IInteractor<SetScarcityLevelCommand, Result> _setScarcity = setScarcity;
    private readonly IInteractor<RemoveScarcityLevelCommand, Result> _removeScarcity =
        removeScarcity;
    private readonly IInteractor<LoanToolCommand, Result> _loanTool = loanTool;
    private readonly IInteractor<ReturnToolCommand, Result> _returnTool = returnTool;
    private readonly IInteractor<MarkToolForRepairCommand, Result> _markForRepair = markForRepair;
    private readonly IInteractor<CompleteRepairCommand, Result> _completeRepair = completeRepair;
    private readonly IInteractor<MarkToolAsLostCommand, Result> _markAsLost = markAsLost;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ToolId), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolId>> Register(
        RegisterToolRequest request,
        CancellationToken ct
    )
    {
        var manufacturer = await _manufacturerRepository.GetByIdAsync(
            new ManufacturerId(request.ManufacturerId),
            ct
        );
        if (manufacturer is null)
            return NotFound(new ErrorResponse("ManufacturerNotFound", "Manufacturer not found."));

        var command = new RegisterToolCommand(
            request.Model,
            request.Description,
            manufacturer,
            (ToolType)request.ToolType,
            (AmortizationRate)request.AmortizationRate,
            new MemberId(request.OwnerId),
            request.SerialNumber,
            (Condition)request.InitialCondition,
            request.Metadata
        );

        var result = await _registerTool.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToErrorResult(result.Error);
    }

    [HttpPost("update-details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateDetails(
        UpdateToolDetailsRequest request,
        CancellationToken ct
    )
    {
        var manufacturer = await _manufacturerRepository.GetByIdAsync(
            new ManufacturerId(request.ManufacturerId),
            ct
        );
        if (manufacturer is null)
            return NotFound(new ErrorResponse("ManufacturerNotFound", "Manufacturer not found."));

        var command = new UpdateToolDetailsCommand(
            new ToolId(request.ToolId),
            request.Model,
            request.Description,
            manufacturer,
            (ToolType)request.ToolType,
            (AmortizationRate)request.AmortizationRate,
            request.Metadata
        );

        var result = await _updateToolDetails.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("scarcity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetScarcity(
        SetScarcityLevelRequest request,
        CancellationToken ct
    )
    {
        var command = new SetScarcityLevelCommand(
            new ToolId(request.ToolId),
            new LocationId(request.LocationId),
            (ScarcityLevel)request.Level
        );

        var result = await _setScarcity.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("remove-scarcity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveScarcity(
        RemoveScarcityLevelRequest request,
        CancellationToken ct
    )
    {
        var command = new RemoveScarcityLevelCommand(
            new ToolId(request.ToolId),
            new LocationId(request.LocationId)
        );

        var result = await _removeScarcity.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("loan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Loan(LoanToolRequest request, CancellationToken ct)
    {
        var command = new LoanToolCommand(
            new InventoryItemId(request.ItemId),
            new MemberId(request.MemberId)
        );

        var result = await _loanTool.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("return")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Return(ReturnToolRequest request, CancellationToken ct)
    {
        var command = new ReturnToolCommand(
            new InventoryItemId(request.ItemId),
            (Condition)request.ReturnedCondition
        );

        var result = await _returnTool.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("mark-for-repair")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkForRepair(
        MarkToolForRepairRequest request,
        CancellationToken ct
    )
    {
        var command = new MarkToolForRepairCommand(
            new InventoryItemId(request.ItemId),
            new MemberId(request.ReportedById),
            request.Description
        );

        var result = await _markForRepair.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("complete-repair")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CompleteRepair(
        CompleteRepairRequest request,
        CancellationToken ct
    )
    {
        var command = new CompleteRepairCommand(
            new MaintenanceRecordId(request.RecordId),
            new InventoryItemId(request.ItemId),
            new MemberId(request.CompletedById),
            (Condition)request.NewCondition
        );

        var result = await _completeRepair.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("mark-lost")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsLost(MarkToolAsLostRequest request, CancellationToken ct)
    {
        var command = new MarkToolAsLostCommand(
            new InventoryItemId(request.ItemId),
            new MemberId(request.ReporterId)
        );

        var result = await _markAsLost.ExecuteAsync(command, ct);
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
