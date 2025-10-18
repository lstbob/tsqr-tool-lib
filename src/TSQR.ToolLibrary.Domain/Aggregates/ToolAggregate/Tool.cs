namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a tool in the tool library system.
/// </summary>
public class Tool : Entity<ToolId>
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
        Model = string.IsNullOrWhiteSpace(model) ?
            throw new ArgumentException("Model is invalid.") : model;

        Description = string.IsNullOrWhiteSpace(description) ?
            throw new ArgumentException("Description is invalid.") : description;

        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));

        Type = type.Equals(ToolType.NotSet) 
            ? throw new ArgumentException("Tool type cannot be Invalid.", nameof(type)) 
            : type;

        Metadata = metadata;
    } 

    public string Model { get; }
    public string Description { get; }
    public Manufacturer Manufacturer { get; }
    public ToolType Type { get; }
    public string? Metadata { get; }

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

}
