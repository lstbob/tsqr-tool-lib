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
        string? metadata = null) : base(id)
    {
        Model = model.Validate(nameof(model));
        Description = description.Validate(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        Type = type.ValidateDefined(nameof(type)).ValidateNotDefault(nameof(type));
        AmortizationRate = amortizationRate.ValidateDefined(nameof(amortizationRate)).ValidateNotDefault(nameof(amortizationRate));
        Metadata = metadata;
    }

    public string Model { get; private set; }
    public string Description { get; private set; }
    public Manufacturer Manufacturer { get; private set; }
    public ToolType Type { get; private set; }
    public AmortizationRate AmortizationRate { get; private set; }
    public string? Metadata { get; private set; }

    private readonly Dictionary<LocationId, ScarcityLevel> _scarcityByLocation = [];
    public IReadOnlyDictionary<LocationId, ScarcityLevel> ScarcityByLocation => _scarcityByLocation.AsReadOnly();

    public static Tool Create(
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null)
    {
        return new(new ToolId(default), model, description, manufacturer, type, amortizationRate, metadata);
    }

    public static Tool Create(
        ToolId id,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        AmortizationRate amortizationRate,
        string? metadata = null)
    {
        return new(id, model, description, manufacturer, type, amortizationRate, metadata);
    }

    public void UpdateToolDetails(string model, string description, Manufacturer manufacturer,
            ToolType type, AmortizationRate amortizationRate, string? metadata = null)
    {
        Model = model.Validate(nameof(model));
        Description = description.Validate(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        Type = type.ValidateDefined(nameof(type)).ValidateNotDefault(nameof(type));
        AmortizationRate = amortizationRate.ValidateDefined(nameof(amortizationRate)).ValidateNotDefault(nameof(amortizationRate));
        Metadata = metadata;
    }

    public void SetScarcityLevel(LocationId locationId, ScarcityLevel level)
    {
        ArgumentNullException.ThrowIfNull(locationId);
        level.ValidateDefined(nameof(level)).ValidateNotDefault(nameof(level));
        _scarcityByLocation[locationId] = level;
    }

    public void RemoveScarcityLevel(LocationId locationId)
    {
        _scarcityByLocation.Remove(locationId);
    }
}
