using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record RegisterToolRequest(
    [Required][StringLength(200)] string Model,
    [Required][StringLength(2000)] string Description,
    [Required] int ManufacturerId,
    [Range(1, 99)] int ToolType,
    [Range(1, 99)] int AmortizationRate,
    [Required] int OwnerId,
    [Required][StringLength(200)] string SerialNumber,
    [Range(1, 99)] int InitialCondition,
    string? Metadata = null);
