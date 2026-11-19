using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record LinkMemberAccountRequest(
    [Required] int MemberId,
    [Required] Guid AuthSubject);
