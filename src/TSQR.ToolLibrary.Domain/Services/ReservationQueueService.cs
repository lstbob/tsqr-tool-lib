namespace TSQR.ToolLibrary.Domain.Services;

public class ReservationQueueService
{
    public int CalculateNextQueuePosition(IReadOnlyCollection<Reservation> activeReservations)
    {
        return activeReservations.Count + 1;
    }

    public Reservation? GetNextInLine(IReadOnlyCollection<Reservation> activeReservations)
    {
        return activeReservations
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .OrderBy(r => r.QueuePosition)
            .ThenBy(r => r.ReservationDate)
            .FirstOrDefault();
    }

    public IReadOnlyCollection<Reservation> ShiftQueueAfterCancellation(
        IReadOnlyCollection<Reservation> activeReservations,
        Reservation cancelledReservation)
    {
        foreach (var reservation in activeReservations
            .Where(r => r.QueuePosition > cancelledReservation.QueuePosition))
        {
            reservation.MoveDownInQueue();
        }

        return activeReservations;
    }
}
