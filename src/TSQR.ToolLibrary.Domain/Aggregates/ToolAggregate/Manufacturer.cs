namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a manufacturer in the tool library system.
/// </summary>
public class Manufacturer : Entity<ManufacturerId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manufacturer"/> class.
    /// </summary>
    private Manufacturer(ManufacturerId id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; }

    public static Manufacturer Create(ManufacturerId id, string name)
    {
        return new(id, name);
    }
}

