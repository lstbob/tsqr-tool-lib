namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

public class MembershipRecord
{
    private MembershipRecord(
        DateTime startDate,
        DateTime? endDate,
        MembershipType membershipType)
    {
        StartDate = startDate;
        EndDate = endDate;
        MembershipType = membershipType;
    }

    public DateTime StartDate { get; }
    public DateTime? EndDate { get; private set; }
    public MembershipType MembershipType { get; private set; }

    public static MembershipRecord Create(DateTime startDate, MembershipType membershipType)
    {
        return new(startDate, null, membershipType);
    }

    public void Renew(DateTime newEndDate)
    {
        EndDate = newEndDate;
    }

    public bool IsActive()
    {
        return EndDate is null || EndDate >= DateTime.UtcNow;
    }
}
