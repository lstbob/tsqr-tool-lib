using System.ComponentModel.DataAnnotations;

namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record CompleteReservationRequest(
    [Required] int ReservationId);
