namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

public class Policy : Entity<PolicyId>
{
    private const int maxAllowedLoanDurationDays = 7;
    private const int maxAllowedRenewalCount = 2;
    private const int maxAllowedLoanReservationDays = 14;

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
        ToolType = toolType;
        LocationId = locationId;
        Name = name;
        Description = description;
        LateFeePerDay = lateFeePerDay;
        MaxLoanDurationDays = maxLoanDurationDays;
        MaxRenewalCount = maxRenewalCount;
        MaxLoanRerservationDays = maxLoanReservationDays;
    }

    public ToolType ToolType { get; }
    public LocationId LocationId { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal LateFeePerDay { get; private set; }
    public int MaxLoanDurationDays { get; private set; }
    public int MaxRenewalCount { get; private set; }
    public int MaxLoanRerservationDays { get; private set; }

    public static Result<Policy> Create(
        ToolType toolType,
        LocationId locationId,
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

        if (locationId is null)
            return new ValidationError(nameof(locationId), "Location ID is required.");

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > maxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > maxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > maxAllowedLoanReservationDays)
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
        LocationId locationId,
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

        if (locationId is null)
            return new ValidationError(nameof(locationId), "Location ID is required.");

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > maxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > maxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > maxAllowedLoanReservationDays)
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

        if (maxLoanDurationDays <= 0 || maxLoanDurationDays > maxAllowedLoanDurationDays)
            return new ValidationError(nameof(maxLoanDurationDays), "Invalid max loan duration days.");

        if (maxRenewalCount <= 0 || maxRenewalCount > maxAllowedRenewalCount)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count cannot be negative.");

        if (maxLoanReservationDays <= 0 || maxLoanReservationDays > maxAllowedLoanReservationDays)
            return new ValidationError(nameof(maxLoanReservationDays), "Max loan reservation days cannot be negative.");

        Name = nameResult.Value;
        Description = descriptionResult.Value;
        LateFeePerDay = lateFeePerDay;
        MaxLoanDurationDays = maxLoanDurationDays;
        MaxRenewalCount = maxRenewalCount;
        MaxLoanRerservationDays = maxLoanReservationDays;

        return Result.Success();
    }
}
