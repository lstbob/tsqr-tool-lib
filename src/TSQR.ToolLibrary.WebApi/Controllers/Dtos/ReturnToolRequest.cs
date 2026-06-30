using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ReturnToolRequest(
    [Required] int ItemId,
    [Range(1, 99)] int ReturnedCondition);
