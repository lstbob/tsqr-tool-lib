using System.Reflection.Metadata.Ecma335;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

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
        string manufacturer,
        ToolType type,
        string? metadata = null) : base(id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        Type = type;
        Metadata = metadata;
    } 

    public ToolId Id { get; }
    public string Model { get; }
    public string Description { get; }
    public string Manufacturer { get; }
    public ToolType Type { get; }
    public string? Metadata { get; }

    /// <summary>
    /// Factory method to create a new instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        string model,
        string description,
        string manufacturer,
        ToolType type,
        string? metadata = null)
    {
        return new Tool(new ToolId(default), model, description, manufacturer, type, metadata);
    }
    
    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="Tool"/> class.
    /// </summary>
    public static Tool Create(
        ToolId id,
        string model,
        string description,
        string manufacturer,
        string serialNumber,
        ToolType type,
        string? metadata = null)
    {
        return new (id, model, description, manufacturer, type, metadata);
    }

}

