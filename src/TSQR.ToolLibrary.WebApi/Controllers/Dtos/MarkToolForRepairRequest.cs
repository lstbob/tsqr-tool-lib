using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record MarkToolForRepairRequest(
    [Required] int ItemId,
    [Required] int ReportedById,
    [Required][StringLength(2000)] string Description);
