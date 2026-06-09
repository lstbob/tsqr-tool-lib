using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

public sealed class DapperReservationRepository : Repository<Reservation, ReservationId>, IReservationRepository
{
    public DapperReservationRepository(IDatabaseUnitOfWork uow, IEntityMapping<Reservation> mapping) : base(uow, mapping)
    {
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(InventoryItemId itemId, CancellationToken cancellationToken = default)
    {
        var rows = await Database.QueryAsync<ReservationRow>(
            "SELECT Id, ItemId, MemberId, ReservationDate, ExpiryDate, Status, IsConfirmed, QueuePosition FROM Reservations WHERE ItemId = @ItemId",
            new { ItemId = itemId.Value });

        return rows.Select(r => Reservation.Create(
            r.Id, r.ItemId, r.MemberId, r.ReservationDate,
            r.ExpiryDate, r.Status, r.IsConfirmed, r.QueuePosition)).ToList();
    }
}
