namespace TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

public interface IReservationRepository : IRepository<Reservation, ReservationId>
{
    Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(InventoryItemId itemId, CancellationToken cancellationToken = default);
}
