using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize] // state-changing member operations require an authenticated caller
[Route("api/members")]
public sealed class MembersCommandController(
    IInteractor<RegisterMemberCommand, Result<MemberId>> registerMember,
    IInteractor<RequestMemberAccessCommand, Result> requestAccess,
    IInteractor<ApproveMemberAccessCommand, Result> approveAccess,
    IInteractor<DenyMemberAccessCommand, Result> denyAccess,
    IInteractor<VerifyMemberCommand, Result> verifyMember,
    IInteractor<SuspendMemberCommand, Result> suspend,
    IInteractor<BanMemberCommand, Result> ban,
    IInteractor<ReinstateMemberCommand, Result> reinstate
) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(MemberId), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberId>> Register(
        RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        var record =
            request.MembershipStartDate.HasValue && request.MembershipType.HasValue
                ? MembershipRecord.Create(
                    request.MembershipStartDate.Value,
                    (MembershipType)request.MembershipType.Value
                )
                : null;

        var command = new RegisterMemberCommand(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Age,
            request.Address,
            request.Email,
            request.PhoneNumber,
            (MemberStatus)request.Status,
            record
        );

        var result = await registerMember.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToErrorResult(result.Error);
    }

    [HttpPost("request-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RequestAccess(
        RequestMemberAccessRequest request,
        CancellationToken ct
    )
    {
        var command = new RequestMemberAccessCommand(
            new MemberId(request.MemberId),
            (IdentificationType)request.IdentificationType,
            request.IdentificationReference
        );

        var result = await requestAccess.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("approve-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveAccess(
        ApproveMemberAccessRequest request,
        CancellationToken ct
    )
    {
        var command = new ApproveMemberAccessCommand(
            new MemberId(request.MemberId),
            new MemberId(request.AdminId)
        );

        var result = await approveAccess.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("deny-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DenyAccess(
        DenyMemberAccessRequest request,
        CancellationToken ct
    )
    {
        var command = new DenyMemberAccessCommand(new MemberId(request.MemberId));

        var result = await denyAccess.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Verify(VerifyMemberRequest request, CancellationToken ct)
    {
        var command = new VerifyMemberCommand(
            new MemberId(request.MemberId),
            new MemberId(request.AdminId)
        );

        var result = await verifyMember.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Suspend(SuspendMemberRequest request, CancellationToken ct)
    {
        var command = new SuspendMemberCommand(new MemberId(request.MemberId));

        var result = await suspend.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Ban(BanMemberRequest request, CancellationToken ct)
    {
        var command = new BanMemberCommand(new MemberId(request.MemberId));

        var result = await ban.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("reinstate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reinstate(ReinstateMemberRequest request, CancellationToken ct)
    {
        var command = new ReinstateMemberCommand(new MemberId(request.MemberId));

        var result = await reinstate.ExecuteAsync(command, ct);
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
