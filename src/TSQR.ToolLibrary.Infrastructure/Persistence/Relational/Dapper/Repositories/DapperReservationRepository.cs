using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class DapperReservationRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Reservation> mapping,
    ISqlDialect dialect)
    : SqlRepository<Reservation, ReservationId>(uow, mapping, dialect), IReservationRepository
{
    public async Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
        InventoryItemId itemId,
        CancellationToken cancellationToken = default
    )
    {
        var rows = await Database.QueryAsync<ReservationRow>(
            "SELECT Id, ItemId, MemberId, ReservationDate, ExpiryDate, Status, IsConfirmed, QueuePosition FROM Reservations WHERE ItemId = @ItemId",
            new { ItemId = itemId.Value }
        );

        return rows.Select(r =>
                Reservation.Create(
                    r.Id,
                    r.ItemId,
                    r.MemberId,
                    r.ReservationDate,
                    r.ExpiryDate,
                    r.Status,
                    r.IsConfirmed,
                    r.QueuePosition
                )
            )
            .ToList();
    }
}
