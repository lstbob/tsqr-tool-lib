using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ReserveToolRequest(
    [Required] int ItemId,
    [Required] int MemberId,
    [Required] DateTime ReservationDate);
