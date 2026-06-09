using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ActivateReservationRequest(
    [Required] int ReservationId);
