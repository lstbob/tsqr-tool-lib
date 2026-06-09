using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record SuspendMemberRequest(
    [Required] int MemberId);
