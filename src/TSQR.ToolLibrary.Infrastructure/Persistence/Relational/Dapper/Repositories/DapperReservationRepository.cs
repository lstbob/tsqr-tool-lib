using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class DapperReservationRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Reservation, ReservationId> mapping,
    ISqlDialect dialect,
    IDomainEventCollector eventCollector)
    : SqlRepository<Reservation, ReservationId>(uow, mapping, dialect, eventCollector), IReservationRepository
{
    public async Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
        InventoryItemId itemId,
        CancellationToken cancellationToken = default
    )
    {
        var rows = await Database.QueryAsync<ReservationRow>(
            @"SELECT Id, ItemId, MemberId, ReservationDate, ExpiryDate, Status,
                     IsConfirmed, QueuePosition, xmin::text::bigint AS RowVersion
              FROM Reservations WHERE ItemId = @ItemId",
            new { ItemId = itemId.Value }
        );

        return rows.Select(r =>
            {
                var reservation = Reservation.Create(
                    r.Id,
                    r.ItemId,
                    r.MemberId,
                    r.ReservationDate,
                    r.ExpiryDate,
                    r.Status,
                    r.IsConfirmed,
                    r.QueuePosition);

                reservation.SetRowVersion(r.RowVersion);
                return reservation;
            })
            .ToList();
    }
}
