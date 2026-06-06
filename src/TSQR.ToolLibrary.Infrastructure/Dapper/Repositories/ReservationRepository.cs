namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

internal sealed record ReservationRow(
    ReservationId Id,
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate,
    DateTime ExpiryDate,
    ReservationStatus Status,
    bool IsConfirmed,
    int QueuePosition);

public class ReservationRepository : IRepository<Reservation, ReservationId>
{
    private readonly DapperUnitOfWork _uow;

    public ReservationRepository(DapperUnitOfWork uow) => _uow = uow;

    public IUnitOfWork UnitOfWork => _uow;

    public async Task<Reservation?> GetByIdAsync(ReservationId id, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var row = await _uow.Connection.QuerySingleOrDefaultAsync<ReservationRow>(
            @"SELECT Id, ItemId, MemberId, ReservationDate, ExpiryDate,
                     Status, IsConfirmed, QueuePosition
              FROM Reservations WHERE Id = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

        if (row is null) return null;

        return Reservation.Create(
            row.Id,
            row.ItemId,
            row.MemberId,
            row.ReservationDate,
            row.ExpiryDate,
            row.Status,
            row.IsConfirmed,
            row.QueuePosition);
    }

    public async Task AddAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var id = await _uow.Connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Reservations (ItemId, MemberId, ReservationDate, ExpiryDate, Status, IsConfirmed, QueuePosition)
              VALUES (@ItemId, @MemberId, @ReservationDate, @ExpiryDate, @Status, @IsConfirmed, @QueuePosition);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                ItemId = entity.ItemId.Value,
                MemberId = entity.MemberId.Value,
                entity.ReservationDate,
                entity.ExpiryDate,
                Status = entity.Status,
                entity.IsConfirmed,
                entity.QueuePosition
            },
            transaction: _uow.Transaction);

        entity.SetAssignedId(new ReservationId(id));
    }

    public void Update(Reservation entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            @"UPDATE Reservations
              SET ItemId = @ItemId, MemberId = @MemberId,
                  ReservationDate = @ReservationDate, ExpiryDate = @ExpiryDate,
                  Status = @Status, IsConfirmed = @IsConfirmed,
                  QueuePosition = @QueuePosition
              WHERE Id = @Id",
            new
            {
                Id = entity.Id.Value,
                ItemId = entity.ItemId.Value,
                MemberId = entity.MemberId.Value,
                entity.ReservationDate,
                entity.ExpiryDate,
                Status = entity.Status,
                entity.IsConfirmed,
                entity.QueuePosition
            },
            transaction: _uow.Transaction);
    }

    public void Delete(Reservation entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            "DELETE FROM Reservations WHERE Id = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);
    }
}
