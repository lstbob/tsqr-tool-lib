using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ReinstateMemberRequest(
    [Required] int MemberId);
