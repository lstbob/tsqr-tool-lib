using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record MarkToolAsLostRequest(
    [Required] int ItemId,
    [Required] int ReporterId);
