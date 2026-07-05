namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

public class Tool : Entity<ToolId>, IAggregateRoot
{
    private Tool(
        ToolId id,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null,
        int communityId = 0) : base(id)
    {
        Model = model;
        Description = description;
        Manufacturer = manufacturer;
        Type = type;
        AmortizationRate = amortizationRate;
        Metadata = metadata;
        CommunityId = communityId;
    }

    public string Model { get; private set; }
    public string Description { get; private set; }
    public Manufacturer Manufacturer { get; private set; }
    public ToolType Type { get; private set; }
    public AmortizationRate AmortizationRate { get; private set; }
    public string? Metadata { get; private set; }
    public int CommunityId { get; private set; }

    private readonly Dictionary<LocationId, ScarcityLevel> _scarcityByLocation = [];
    public IReadOnlyDictionary<LocationId, ScarcityLevel> ScarcityByLocation => _scarcityByLocation.AsReadOnly();

    public static Result<Tool> Create(
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null,
        int communityId = 0)
    {
        var modelResult = model.Validate(nameof(model));
        if (modelResult.IsFailure) return modelResult.Error;

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure) return descriptionResult.Error;

        if (manufacturer is null)
            return new ValidationError(nameof(manufacturer), "Manufacturer is required.");

        var typeResult = type.ValidateDefined(nameof(type));
        if (typeResult.IsFailure) return typeResult.Error;

        var typeNotDefaultResult = type.ValidateNotDefault(nameof(type));
        if (typeNotDefaultResult.IsFailure) return typeNotDefaultResult.Error;

        var rateResult = amortizationRate.ValidateDefined(nameof(amortizationRate));
        if (rateResult.IsFailure) return rateResult.Error;

        var rateNotDefaultResult = amortizationRate.ValidateNotDefault(nameof(amortizationRate));
        if (rateNotDefaultResult.IsFailure) return rateNotDefaultResult.Error;

        return new Tool(new ToolId(default), modelResult.Value, descriptionResult.Value, manufacturer, type, amortizationRate, metadata, communityId);
    }

    public static Tool Create(
        ToolId id,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null,
        int communityId = 0)
    {
        return new Tool(id, model, description, manufacturer, type, amortizationRate, metadata, communityId);
    }

    /// <summary>
    /// Factory for registering a new tool with a member. Performs the same validation
    /// as <see cref="Create(string, string, Manufacturer, ToolType, AmortizationRate, string?)"/>
    /// and additionally raises <see cref="ToolRegisteredEvent"/> so the application layer
    /// does not raise domain events itself.
    /// </summary>
    public static Result<Tool> Register(
        MemberId ownerId,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null,
        int communityId = 0)
    {
        if (ownerId is null)
            return new ValidationError(nameof(ownerId), "Owner ID is required.");

        var createResult = Create(model, description, manufacturer, type, amortizationRate, metadata, communityId);
        if (createResult.IsFailure)
            return createResult.Error;

        var tool = createResult.Value;
        tool.AddDomainEvent(new ToolRegisteredEvent(tool.Id, ownerId, model, type));
        return tool;
    }

    public Result UpdateToolDetails(string model, string description, Manufacturer manufacturer,
            ToolType type, AmortizationRate amortizationRate, string? metadata = null)
    {
        var modelResult = model.Validate(nameof(model));
        if (modelResult.IsFailure) return modelResult.Error;

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure) return descriptionResult.Error;

        if (manufacturer is null)
            return new ValidationError(nameof(manufacturer), "Manufacturer is required.");

        var typeResult = type.ValidateDefined(nameof(type));
        if (typeResult.IsFailure) return typeResult.Error;

        var typeNotDefaultResult = type.ValidateNotDefault(nameof(type));
        if (typeNotDefaultResult.IsFailure) return typeNotDefaultResult.Error;

        var rateResult = amortizationRate.ValidateDefined(nameof(amortizationRate));
        if (rateResult.IsFailure) return rateResult.Error;

        var rateNotDefaultResult = amortizationRate.ValidateNotDefault(nameof(amortizationRate));
        if (rateNotDefaultResult.IsFailure) return rateNotDefaultResult.Error;

        Model = modelResult.Value;
        Description = descriptionResult.Value;
        Manufacturer = manufacturer;
        Type = type;
        AmortizationRate = amortizationRate;
        Metadata = metadata;

        return Result.Success();
    }

    public Result SetScarcityLevel(LocationId locationId, ScarcityLevel level)
    {
        if (locationId is null)
            return new ValidationError(nameof(locationId), "Location ID is required.");

        var levelResult = level.ValidateDefined(nameof(level));
        if (levelResult.IsFailure) return levelResult.Error;

        var notDefaultResult = level.ValidateNotDefault(nameof(level));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        _scarcityByLocation[locationId] = level;
        return Result.Success();
    }

    public void RemoveScarcityLevel(LocationId locationId)
    {
        _scarcityByLocation.Remove(locationId);
    }
}
