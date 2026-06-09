using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record MarkLoanAsNotReturnedRequest(
    [Required] int LoanId);
