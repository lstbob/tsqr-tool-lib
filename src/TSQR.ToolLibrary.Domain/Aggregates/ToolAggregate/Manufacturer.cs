namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a manufacturer in the tool library system.
/// </summary>
public class Manufacturer : Entity<ManufacturerId>
{
    private Manufacturer(ManufacturerId id, string name) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name) 
            ? throw new ArgumentException("Manufacturer name is invalid.", nameof(name)) 
            : name;
    }

    public string Name { get; }
}

