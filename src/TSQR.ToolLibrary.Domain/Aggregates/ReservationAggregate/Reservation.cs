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
        int queuePosition,
        int communityId = 0) : base(id)
    {
        ItemId = itemId;
        MemberId = memberId;
        ReservationDate = reservationDate;
        ExpiryDate = expiryDate;
        Status = status;
        IsConfirmed = isConfirmed;
        QueuePosition = queuePosition;
        CommunityId = communityId;
    }

    public InventoryItemId ItemId { get; }
    public MemberId MemberId { get; }
    public DateTime ReservationDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public ReservationStatus Status { get; private set; }
    public bool IsConfirmed { get; private set; }
    public int QueuePosition { get; private set; }
    public int CommunityId { get; private set; }

    public static Result<Reservation> Create(
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        DateTime expiryDate,
        int queuePosition,
        int communityId = 0)
    {
        if (itemId is null)
            return new ValidationError(nameof(itemId), "Item ID is required.");
        if (memberId is null)
            return new ValidationError(nameof(memberId), "Member ID is required.");
        if (reservationDate == default)
            return new ValidationError(nameof(reservationDate), "Reservation date is required.");
        if (expiryDate == default)
            return new ValidationError(nameof(expiryDate), "Expiry date is required.");

        return new Reservation(
            new ReservationId(default),
            itemId,
            memberId,
            reservationDate,
            expiryDate,
            ReservationStatus.Pending,
            false,
            queuePosition,
            communityId);
    }

    public static Result<Reservation> Create(
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        int communityId = 0)
    {
        var expiryDate = reservationDate.AddDays(14);
        var reservation = Create(itemId, memberId, reservationDate, expiryDate, 1, communityId);
        if (reservation.IsSuccess)
            reservation.Value.AddDomainEvent(new ReservationCreatedDomainEvent(
                reservation.Value.Id, itemId, memberId, reservationDate));
        return reservation;
    }

    public static Reservation Create(
        ReservationId id,
        InventoryItemId itemId,
        MemberId memberId,
        DateTime reservationDate,
        DateTime expiryDate,
        ReservationStatus status,
        bool isConfirmed,
        int queuePosition,
        int communityId = 0)
    {
        return new Reservation(
            id,
            itemId,
            memberId,
            reservationDate,
            expiryDate,
            status,
            isConfirmed,
            queuePosition,
            communityId);
    }

    public Result ConfirmPickup()
    {
        if (Status != ReservationStatus.Pending)
            return new DomainError(nameof(Status), "Only pending reservations can be confirmed.");

        IsConfirmed = true;
        Status = ReservationStatus.Confirmed;

        AddDomainEvent(new ReservationConfirmedEvent(Id, ItemId, MemberId));
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status != ReservationStatus.Confirmed)
            return new DomainError(nameof(Status), "Only confirmed reservations can be activated.");

        Status = ReservationStatus.Active;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ReservationStatus.Cancelled || Status == ReservationStatus.Completed)
            return new DomainError(nameof(Status), "Reservation is already cancelled or completed.");

        Status = ReservationStatus.Cancelled;

        AddDomainEvent(new ReservationCancelledEvent(Id, ItemId, MemberId));
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != ReservationStatus.Active)
            return new DomainError(nameof(Status), "Only active reservations can be completed.");

        Status = ReservationStatus.Completed;
        return Result.Success();
    }

    public void MoveDownInQueue()
    {
        QueuePosition++;
    }

    public void NotifyNextInLine(string reason)
    {
        AddDomainEvent(new NextInLineNotificationEvent(Id, ItemId, MemberId, reason));
    }
}
