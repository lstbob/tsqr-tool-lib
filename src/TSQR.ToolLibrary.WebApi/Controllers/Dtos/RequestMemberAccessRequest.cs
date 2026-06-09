using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record RequestMemberAccessRequest(
    [Required] int MemberId,
    [Range(1, 99)] int IdentificationType,
    [Required][StringLength(200)] string IdentificationReference);
