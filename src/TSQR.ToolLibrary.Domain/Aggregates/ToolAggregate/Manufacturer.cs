namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a manufacturer in the tool library system.
/// </summary>
public class Manufacturer : Entity<ManufcaturerId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manufacturer"/> class.
    /// </summary>
    private Manufacturer(ManufcaturerId id, string name) : base(id)
    {
        Name = name.Validate(nameof(name));
    }

    public string Name { get; }

    public static Manufacturer Create(ManufcaturerId id, string name)
    {
        return new(id, name);
    }
}

