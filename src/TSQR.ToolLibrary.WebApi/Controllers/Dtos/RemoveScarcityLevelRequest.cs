using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record RemoveScarcityLevelRequest(
    [Required] int ToolId,
    [Required] int LocationId);
