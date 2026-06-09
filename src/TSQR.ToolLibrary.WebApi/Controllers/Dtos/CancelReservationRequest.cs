using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record CancelReservationRequest(
    [Required] int ReservationId);
