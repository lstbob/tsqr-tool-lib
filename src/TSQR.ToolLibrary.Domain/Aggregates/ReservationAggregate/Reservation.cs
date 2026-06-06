namespace TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

public class Reservation : Entity<ReservationId>, IAggregateRoot
{
    private Reservation(
        ReservationId id,
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        DateTime expiryDate,
        ReservationStatus status,
        bool isConfirmed,
        int queuePosition) : base(id)
    {
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));
        ReservationDate = reservationDate;
        ExpiryDate = expiryDate;
        Status = status.ValidateDefined(nameof(status)).ValidateNotDefault(nameof(status));
        IsConfirmed = isConfirmed;
        QueuePosition = queuePosition;
    }

    public InventoryItemId ItemId { get; }
    public MemberId MemberId { get; }
    public DateTime ReservationDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public ReservationStatus Status { get; private set; }
    public bool IsConfirmed { get; private set; }
    public int QueuePosition { get; private set; }

    public static Reservation Create(
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        DateTime expiryDate,
        int queuePosition)
    {
        return new(
            new ReservationId(default),
            itemId,
            memberId,
            reservationDate,
            expiryDate,
            ReservationStatus.Pending,
            false,
            queuePosition);
    }

    public static Reservation Create(
        ReservationId id,
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        DateTime expiryDate,
        ReservationStatus status,
        bool isConfirmed,
        int queuePosition)
    {
        return new(
            id,
            itemId,
            memberId,
            reservationDate,
            expiryDate,
            status,
            isConfirmed,
            queuePosition);
    }

    public void ConfirmPickup()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be confirmed.");

        IsConfirmed = true;
        Status = ReservationStatus.Confirmed;

        AddDomainEvent(new ReservationConfirmedEvent(Id, ItemId, MemberId));
    }

    public void Activate()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed reservations can be activated.");

        Status = ReservationStatus.Active;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled || Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Reservation is already cancelled or completed.");

        Status = ReservationStatus.Cancelled;

        AddDomainEvent(new ReservationCancelledEvent(Id, ItemId, MemberId));
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException("Only active reservations can be completed.");

        Status = ReservationStatus.Completed;
    }

    public void MoveDownInQueue()
    {
        QueuePosition++;
    }
}
