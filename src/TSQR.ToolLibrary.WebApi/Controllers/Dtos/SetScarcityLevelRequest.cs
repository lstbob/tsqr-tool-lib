using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record SetScarcityLevelRequest(
    [Required] int ToolId,
    [Required] int LocationId,
    [Range(1, 99)] int Level);
