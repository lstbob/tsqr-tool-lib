namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents the domain entity for a Tool.
/// </summary>
public class Tool : Entity<ToolId>, IAggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tool"/> class.
    /// </summary>
    private Tool(
        ToolId id,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        string? metadata = null) : base(id)
    {
        Model = model.Validate(nameof(model));

        Description = description.Validate(nameof(description)); 

        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));

        Type = type
            .ValidateDefined(nameof(type))
            .ValidateNotDefault(nameof(type)); 

        Metadata = metadata;
    } 

    public string Model { get; private set; }
    public string Description { get; private set; }
    public Manufacturer Manufacturer { get; private set; }
    public ToolType Type { get; private set; }
    public string? Metadata { get; private set; }

    /// <summary>
    /// Factory method to create a new instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        string? metadata = null)
    {
        return new (new (default), model, description, manufacturer, type, metadata);
    }
    
    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        ToolId id,
        string model,
        string description,
        Manufacturer manufacturer,
        ToolType type,
        string? metadata = null)
    {
        return new (id, model, description, manufacturer, type, metadata);
    }

    /// <summary>
    /// Updates the tool details.
    /// </summary>
    public void UpdateToolDetails(string model, string description, Manufacturer manufacturer,
            ToolType type,  string? metadata = null)
    {
        Model = model.Validate(nameof(model)); 
        Description = description.Validate(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        Type = type
            .ValidateDefined(nameof(type))
            .ValidateNotDefault(nameof(type));    
        Metadata = metadata;
    }
}
