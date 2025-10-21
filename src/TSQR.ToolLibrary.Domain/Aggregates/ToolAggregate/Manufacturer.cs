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

    /// <summary>
    /// Gets the name of the tool manufacturer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Responsible for creating new transient instances of the <see cref="Manufacturer"/> class.
    /// </summary>
    /// <param name="name">The name of the manufacturer</param>
    /// <returns>New transient instance.</returns>
    public static Manufacturer Create(string name)
    {
        return new (new ManufcaturerId(default), name);
    }

    /// <summary>
    /// Responsible for rehydrating existing instances of the <see cref="Manufacturer"/> class.
    /// </summary>
    /// <param name="name">The name of the manufacturer</param>
    /// <returns>New transient instance.</returns>
    public static Manufacturer Create(ManufcaturerId id, string name)
    {
        return new(id, name);
    }

}

