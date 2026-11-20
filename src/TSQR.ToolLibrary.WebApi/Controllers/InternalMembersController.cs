using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.Application.Member.Accounts;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Security;

namespace TSQR.ToolLibrary.WebApi.Controllers;

/// <summary>
/// Service-to-service member lookups. These routes are not proxied by the
/// gateway and require the shared internal API key.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("internal/members")]
public sealed class InternalMembersController(
    IMemberAccountRepository accounts,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("community")]
    [ProducesResponseType(typeof(InternalMemberCommunityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommunity(
        [FromQuery] Guid subject,
        CancellationToken ct)
    {
        if (!InternalApiKeyValidator.IsValid(Request, configuration))
            return Unauthorized(new ErrorResponse("Unauthorized", "Internal API key required."));

        var communityId = await accounts.GetCommunityIdBySubjectAsync(subject, ct);
        return communityId is null
            ? NotFound()
            : Ok(new InternalMemberCommunityResponse(communityId.Value));
    }
}
