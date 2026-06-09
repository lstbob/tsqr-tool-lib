using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersCommandController : ControllerBase
{
    private readonly IInteractor<RegisterMemberCommand, Result<MemberId>> _registerMember;
    private readonly IInteractor<RequestMemberAccessCommand, Result> _requestAccess;
    private readonly IInteractor<ApproveMemberAccessCommand, Result> _approveAccess;
    private readonly IInteractor<DenyMemberAccessCommand, Result> _denyAccess;
    private readonly IInteractor<VerifyMemberCommand, Result> _verifyMember;
    private readonly IInteractor<SuspendMemberCommand, Result> _suspend;
    private readonly IInteractor<BanMemberCommand, Result> _ban;
    private readonly IInteractor<ReinstateMemberCommand, Result> _reinstate;

    public MembersCommandController(
        IInteractor<RegisterMemberCommand, Result<MemberId>> registerMember,
        IInteractor<RequestMemberAccessCommand, Result> requestAccess,
        IInteractor<ApproveMemberAccessCommand, Result> approveAccess,
        IInteractor<DenyMemberAccessCommand, Result> denyAccess,
        IInteractor<VerifyMemberCommand, Result> verifyMember,
        IInteractor<SuspendMemberCommand, Result> suspend,
        IInteractor<BanMemberCommand, Result> ban,
        IInteractor<ReinstateMemberCommand, Result> reinstate)
    {
        _registerMember = registerMember;
        _requestAccess = requestAccess;
        _approveAccess = approveAccess;
        _denyAccess = denyAccess;
        _verifyMember = verifyMember;
        _suspend = suspend;
        _ban = ban;
        _reinstate = reinstate;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(MemberId), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberId>> Register(RegisterMemberRequest request, CancellationToken ct)
    {
        var record = request.MembershipStartDate.HasValue && request.MembershipType.HasValue
            ? MembershipRecord.Create(request.MembershipStartDate.Value, (MembershipType)request.MembershipType.Value)
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
            record);

        var result = await _registerMember.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToErrorResult(result.Error);
    }

    [HttpPost("request-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RequestAccess(RequestMemberAccessRequest request, CancellationToken ct)
    {
        var command = new RequestMemberAccessCommand(
            new MemberId(request.MemberId),
            (IdentificationType)request.IdentificationType,
            request.IdentificationReference);

        var result = await _requestAccess.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("approve-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveAccess(ApproveMemberAccessRequest request, CancellationToken ct)
    {
        var command = new ApproveMemberAccessCommand(
            new MemberId(request.MemberId),
            new MemberId(request.AdminId));

        var result = await _approveAccess.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("deny-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DenyAccess(DenyMemberAccessRequest request, CancellationToken ct)
    {
        var command = new DenyMemberAccessCommand(new MemberId(request.MemberId));

        var result = await _denyAccess.ExecuteAsync(command, ct);
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
            new MemberId(request.AdminId));

        var result = await _verifyMember.ExecuteAsync(command, ct);
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

        var result = await _suspend.ExecuteAsync(command, ct);
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

        var result = await _ban.ExecuteAsync(command, ct);
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

        var result = await _reinstate.ExecuteAsync(command, ct);
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
