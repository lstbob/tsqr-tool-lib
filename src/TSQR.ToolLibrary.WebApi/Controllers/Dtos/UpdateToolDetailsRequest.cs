using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record UpdateToolDetailsRequest(
    [Required] int ToolId,
    [Required][StringLength(200)] string Model,
    [Required][StringLength(2000)] string Description,
    [Required] int ManufacturerId,
    [Range(1, 99)] int ToolType,
    [Range(1, 99)] int AmortizationRate,
    string? Metadata = null);
