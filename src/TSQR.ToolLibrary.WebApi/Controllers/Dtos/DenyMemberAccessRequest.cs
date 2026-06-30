using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record DenyMemberAccessRequest(
    [Required] int MemberId);
