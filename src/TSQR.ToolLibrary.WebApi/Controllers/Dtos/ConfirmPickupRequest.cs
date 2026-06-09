using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ConfirmPickupRequest(
    [Required] int ReservationId);
