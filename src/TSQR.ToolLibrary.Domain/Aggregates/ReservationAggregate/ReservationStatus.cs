namespace TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

public enum ReservationStatus
{
    NotSet = 0,
    Pending,
    Confirmed,
    Active,
    Cancelled,
    Completed
}
