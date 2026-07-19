namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

public class Policy : Entity<PolicyId>, IAggregateRoot
{
    private const int MaxAllowedLoanDurationDays = 7;
    private const int MaxAllowedRenewalCount = 2;
    private const int MaxAllowedLoanReservationDays = 14;

    private Policy(PolicyId id,
        ToolType toolType,
        LocationId? locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays) : base(id)
    {
        ToolType = toolType;
        LocationId = locationId;
        Name = name;
        Description = description;
        LateFeePerDay = lateFeePerDay;
        MaxLoanDurationDays = maxLoanDurationDays;
        MaxRenewalCount = maxRenewalCount;
        MaxLoanReservationDays = maxLoanReservationDays;
    }

    public ToolType ToolType { get; }
    /// <summary>
    /// Optional per-location policy. Null means the policy applies globally
    /// (i.e., to all locations). Lookup keys off (ToolType, LocationId?):
    /// an exact-match policy wins, else the null-location global one.
    /// </summary>
    public LocationId? LocationId { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal LateFeePerDay { get; private set; }
    public int MaxLoanDurationDays { get; private set; }
    public int MaxRenewalCount { get; private set; }
    public int MaxLoanReservationDays { get; private set; }

    public static Result<Policy> Create(
        ToolType toolType,
        LocationId? locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {
        var toolTypeResult = toolType.ValidateDefined(nameof(toolType));
        if (toolTypeResult.IsFailure) return toolTypeResult.Error;

        var notDefaultResult = toolType.ValidateNotDefault(nameof(toolType));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > MaxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > MaxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > MaxAllowedLoanReservationDays)
            return new ValidationError(nameof(maxLoanReservationDays), "Max loan reservation days cannot be negative.");

        var nameResult = name.Validate(nameof(name));
        if (nameResult.IsFailure) return nameResult.Error;

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure) return descriptionResult.Error;

        return new Policy(
            new PolicyId(default),
            toolType,
            locationId,
            nameResult.Value,
            descriptionResult.Value,
            lateFeePerDay,
            maxLoanDurationDays,
            maxRenewalCount,
            maxLoanReservationDays);
    }

    public static Result<Policy> Create(
        PolicyId id,
        ToolType toolType,
        LocationId? locationId,
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {
        var toolTypeResult = toolType.ValidateDefined(nameof(toolType));
        if (toolTypeResult.IsFailure) return toolTypeResult.Error;

        var notDefaultResult = toolType.ValidateNotDefault(nameof(toolType));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > MaxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > MaxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > MaxAllowedLoanReservationDays)
            return new ValidationError(nameof(maxLoanReservationDays), "Max loan reservation days cannot be negative.");

        var nameResult = name.Validate(nameof(name));
        if (nameResult.IsFailure) return nameResult.Error;

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure) return descriptionResult.Error;

        return new Policy(
            id,
            toolType,
            locationId,
            nameResult.Value,
            descriptionResult.Value,
            lateFeePerDay,
            maxLoanDurationDays,
            maxRenewalCount,
            maxLoanReservationDays);
    }

    public Result SetDetails(
        string name,
        string description,
        decimal lateFeePerDay,
        int maxLoanDurationDays,
        int maxRenewalCount,
        int maxLoanReservationDays)
    {
        var nameResult = name.Validate(nameof(name));
        if (nameResult.IsFailure) return nameResult.Error;

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure) return descriptionResult.Error;

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > MaxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > MaxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > MaxAllowedLoanReservationDays)
            return new ValidationError(nameof(maxLoanReservationDays), "Max loan reservation days cannot be negative.");

        Name = nameResult.Value;
        Description = descriptionResult.Value;
        LateFeePerDay = lateFeePerDay;
        MaxLoanDurationDays = maxLoanDurationDays;
        MaxRenewalCount = maxRenewalCount;
        MaxLoanReservationDays = maxLoanReservationDays;

        return Result.Success();
    }
}