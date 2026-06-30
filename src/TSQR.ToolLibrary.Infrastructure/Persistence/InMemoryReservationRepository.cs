namespace TSQR.ToolLibrary.Infrastructure.Persistence;

public class InMemoryReservationRepository
    : InMemoryRepository<Reservation, ReservationId>,
        IReservationRepository
{
    public Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
        InventoryItemId itemId,
        CancellationToken cancellationToken = default
    )
    {
        var reservations = GetAll().Where(r => r.ItemId.Equals(itemId)).ToList().AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<Reservation>>(reservations);
    }
}
