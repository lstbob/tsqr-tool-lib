using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record VerifyMemberRequest(
    [Required] int MemberId,
    [Required] int AdminId);
