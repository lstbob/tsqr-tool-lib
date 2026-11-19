using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.Common.Authorization;
using TSQR.ToolLibrary.Application.Reservation.Commands;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Security;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize] // state-changing reservation operations require an authenticated caller
[Route("api/reservations")]
public sealed class ReservationsCommandController(
    IInteractor<ReserveToolCommand, Result<ReservationId>> reserve,
    IInteractor<ActivateReservationCommand, Result> activate,
    IInteractor<CancelReservationCommand, Result> cancel,
    IInteractor<ConfirmPickupCommand, Result> confirmPickup,
    IInteractor<CompleteReservationCommand, Result> complete,
    ICurrentUser currentUser,
    ICurrentMemberResolver currentMember
) : ControllerBase
{
    private readonly IInteractor<ReserveToolCommand, Result<ReservationId>> _reserve = reserve;
    private readonly IInteractor<ActivateReservationCommand, Result> _activate = activate;
    private readonly IInteractor<CancelReservationCommand, Result> _cancel = cancel;
    private readonly IInteractor<ConfirmPickupCommand, Result> _confirmPickup = confirmPickup;
    private readonly IInteractor<CompleteReservationCommand, Result> _complete = complete;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ICurrentMemberResolver _currentMember = currentMember;

    [HttpPost("create")]
    [Authorize(Policy = Permissions.ReservationsSelfService)]
    [ProducesResponseType(typeof(ReservationId), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationId>> Create(
        ReserveToolRequest request,
        CancellationToken ct
    )
    {
        var command = new ReserveToolCommand(
            new InventoryItemId(request.ItemId),
            new MemberId(request.MemberId),
            request.ReservationDate,
            ActorMemberId: await _currentMember.GetMemberIdAsync(ct),
            CanReserveForOthers: CanManageAnyReservation()
        );

        var result = await _reserve.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToErrorResult(result.Error);
    }

    [HttpPost("activate")]
    [Authorize(Policy = Permissions.ReservationsSelfService)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Activate(
        ActivateReservationRequest request,
        CancellationToken ct
    )
    {
        var command = new ActivateReservationCommand(
            new ReservationId(request.ReservationId),
            await _currentMember.GetMemberIdAsync(ct),
            CanManageAnyReservation());

        var result = await _activate.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("cancel")]
    [Authorize(Policy = Permissions.ReservationsSelfService)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(CancelReservationRequest request, CancellationToken ct)
    {
        var command = new CancelReservationCommand(
            new ReservationId(request.ReservationId),
            await _currentMember.GetMemberIdAsync(ct),
            CanManageAnyReservation());

        var result = await _cancel.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("confirm-pickup")]
    [Authorize(Policy = Permissions.ReservationsSelfService)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ConfirmPickup(
        ConfirmPickupRequest request,
        CancellationToken ct
    )
    {
        var command = new ConfirmPickupCommand(
            new ReservationId(request.ReservationId),
            await _currentMember.GetMemberIdAsync(ct),
            CanManageAnyReservation());

        var result = await _confirmPickup.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("complete")]
    [Authorize(Policy = Permissions.ReservationsComplete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Complete(
        CompleteReservationRequest request,
        CancellationToken ct
    )
    {
        var command = new CompleteReservationCommand(new ReservationId(request.ReservationId));

        var result = await _complete.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    private bool CanManageAnyReservation() =>
        _currentUser.HasPermission(Permissions.ReservationsComplete);

    private ActionResult ToErrorResult(Error error)
    {
        return error switch
        {
            NotFoundError err => NotFound(new ErrorResponse(err.Code, err.Message)),
            ForbiddenError err => StatusCode(
                StatusCodes.Status403Forbidden,
                new ErrorResponse(err.Code, err.Message)),
            _ => BadRequest(new ErrorResponse(error.Code, error.Message)),
        };
    }
}
