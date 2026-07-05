namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

internal sealed record ReservationRow
{
    public ReservationId Id { get; init; }
    public InventoryItemId ItemId { get; init; }
    public MemberId MemberId { get; init; }
    public DateTime ReservationDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public ReservationStatus Status { get; init; }
    public bool IsConfirmed { get; init; }
    public int QueuePosition { get; init; }
}

internal sealed record ReservationInsertDto(
    int ItemId,
    int MemberId,
    DateTime ReservationDate,
    DateTime ExpiryDate,
    ReservationStatus Status,
    bool IsConfirmed,
    int QueuePosition);

internal sealed record ReservationUpdateDto(
    int Id,
    int ItemId,
    int MemberId,
    DateTime ReservationDate,
    DateTime ExpiryDate,
    ReservationStatus Status,
    bool IsConfirmed,
    int QueuePosition);

public sealed class ReservationMapping : ISqlEntityMapping<Reservation>
{
    public string TableName => "Reservations";

    public string InsertSql =>
        @"INSERT INTO Reservations (ItemId, MemberId, ReservationDate, ExpiryDate, Status, IsConfirmed, QueuePosition)
          VALUES (@ItemId, @MemberId, @ReservationDate, @ExpiryDate, @Status, @IsConfirmed, @QueuePosition)
          RETURNING Id";

    public string UpdateSql =>
        @"UPDATE Reservations
          SET ItemId = @ItemId, MemberId = @MemberId,
              ReservationDate = @ReservationDate, ExpiryDate = @ExpiryDate,
              Status = @Status, IsConfirmed = @IsConfirmed,
              QueuePosition = @QueuePosition
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Reservations WHERE Id = @Id";

    public async Task<Reservation?> GetByIdAsync(ISqlConnection db, int id)
    {
        var row = await db.QuerySingleOrDefaultAsync<ReservationRow>(
            @"SELECT Id, ItemId, MemberId, ReservationDate, ExpiryDate,
                     Status, IsConfirmed, QueuePosition
              FROM Reservations WHERE Id = @Id", new { Id = id });

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

    public object ToInsertParameters(Reservation entity) => new ReservationInsertDto(
        entity.ItemId.Value,
        entity.MemberId.Value,
        entity.ReservationDate,
        entity.ExpiryDate,
        entity.Status,
        entity.IsConfirmed,
        entity.QueuePosition);

    public object ToUpdateParameters(Reservation entity) => new ReservationUpdateDto(
        entity.Id.Value,
        entity.ItemId.Value,
        entity.MemberId.Value,
        entity.ReservationDate,
        entity.ExpiryDate,
        entity.Status,
        entity.IsConfirmed,
        entity.QueuePosition);
}
