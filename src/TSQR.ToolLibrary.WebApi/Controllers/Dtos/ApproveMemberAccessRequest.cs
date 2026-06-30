using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ApproveMemberAccessRequest(
    [Required] int MemberId,
    [Required] int AdminId);
