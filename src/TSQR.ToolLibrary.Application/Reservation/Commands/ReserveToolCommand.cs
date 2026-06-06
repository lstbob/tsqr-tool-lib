using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate) : IRequest<ReservationId>;

public class ReserveToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository)
    : IRequestHandler<ReserveToolCommand, ReservationId>
{
    public async Task<ReservationId> Handle(ReserveToolCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory item not found.");

        var maxReservationDate = DateTime.UtcNow.AddDays(28);
        var expiryDate = request.ReservationDate.AddDays(14);

        if (request.ReservationDate > maxReservationDate)
            throw new InvalidOperationException("Reservations can only be made up to 28 days in advance.");

        item.Reserve(request.ReservationDate, request.MemberId);

        var reservation = ReservationAgg.Reservation.Create(
            request.ItemId,
            request.MemberId,
            request.ReservationDate,
            expiryDate,
            1);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
