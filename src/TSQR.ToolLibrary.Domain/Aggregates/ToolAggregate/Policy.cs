namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a policy in the tool library system.
/// </summary>
public class Policy : Entity<PolicyId>
{
    private const int maxAllowedLoanDurationDays = 7;
    private const int maxAllowedRenewalCount = 2;
    private const int maxAllowedLoanReservationDays = 14;

    /// <summary>
    /// Creates a new policy instance.
    /// </summary>
    private Policy(PolicyId id,
        ToolType toolType,
        LocationId locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays) : base(id)
    {
        ToolType = toolType
            .ValidateDefined(nameof(toolType))
            .ValidateNotDefault(nameof(toolType)); 

        LocationId = locationId ?? throw new ArgumentNullException(nameof(locationId));
        
        SetDetails(
            name,
            description,
            lateFeePerDay,
            maxLoanDurationDays,
            maxRenewalCount,
            maxLoanReservationDays); 
    }
   
    public ToolType ToolType { get; }
    public LocationId LocationId {get;}
    public string Name { get; private set; }
    public string Description { get; private set; } 
    public decimal LateFeePerDay { get; private set; }
    public int MaxLoanDurationDays { get; private set; }
    public int MaxRenewalCount { get; private set; }
    public int MaxLoanRerservationDays { get; private set; } 



    /// <summary>
    /// Factory method to create a new instance of the <see cref="Policy"/> class.
    /// </summary>
    public static Policy Create(
        ToolType toolType,
        LocationId locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {
        return new (
            new (default),
            toolType,
            locationId,
            name,
            description,
            lateFeePerDay,
            maxLoanDurationDays,
            maxRenewalCount,
            maxLoanReservationDays);
    }
    
    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="Policy"/> class.
    /// </summary>
    public static Policy Create(
        PolicyId id,
        ToolType toolType,
        LocationId locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {        return new (
            id,
            toolType,
            locationId,
            name,
            description,
            lateFeePerDay,
            maxLoanDurationDays,
            maxRenewalCount,
            maxLoanReservationDays);    
    }


    /// <summary>
    /// Updates the details of the policy.
    /// </summary>
    public void SetDetails(
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {
        Name = name.Validate(nameof(name));

        Description = description.Validate(nameof(description));

        if (lateFeePerDay < 0)
            throw new ArgumentException("Late fee per day cannot be negative.", nameof(lateFeePerDay));

        LateFeePerDay = lateFeePerDay;

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > maxAllowedLoanDurationDays)
            throw new ArgumentException("Invalid max loan duration days", nameof(maxLoanDurationDays));

        MaxLoanDurationDays = maxLoanDurationDays;

        if (maxRenewalCount <= 0 || maxRenewalCount > maxAllowedRenewalCount)
            throw new ArgumentException("Max renewal count cannot be negative.", nameof(maxRenewalCount));

        MaxRenewalCount = maxRenewalCount;

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > maxAllowedLoanReservationDays)
            throw new ArgumentException("Max loan reservation days cannot be negative.", nameof(maxLoanReservationDays));

        MaxLoanRerservationDays = maxLoanReservationDays;
    }
}

