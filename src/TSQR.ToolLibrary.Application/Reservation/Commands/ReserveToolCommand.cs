using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate) : IRequest<Result<ReservationId>>;

public class ReserveToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository)
    : IRequestHandler<ReserveToolCommand, Result<ReservationId>>
{
    public async Task<Result<ReservationId>> Handle(ReserveToolCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(request.ItemId), "Inventory item not found.");

        var maxReservationDate = DateTime.UtcNow.AddDays(28);
        var expiryDate = request.ReservationDate.AddDays(14);

        if (request.ReservationDate > maxReservationDate)
            return new DomainError(nameof(request.ReservationDate), "Reservations can only be made up to 28 days in advance.");

        var reserveResult = item.Reserve(request.ReservationDate, request.MemberId);
        if (reserveResult.IsFailure)
            return reserveResult.Error;

        var reservationResult = ReservationAgg.Reservation.Create(
            request.ItemId,
            request.MemberId,
            request.ReservationDate,
            expiryDate,
            1);

        if (reservationResult.IsFailure)
            return reservationResult.Error;

        var reservation = reservationResult.Value;
        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
