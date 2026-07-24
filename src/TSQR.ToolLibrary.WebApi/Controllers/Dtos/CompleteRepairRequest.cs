using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record CompleteRepairRequest(
    [Required] int ItemId,
    [Required] int CompletedById,
    [Range(1, 99)] int NewCondition);
