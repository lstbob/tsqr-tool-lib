using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record LoanToolRequest(
    [Required] int ItemId,
    [Required] int MemberId);
