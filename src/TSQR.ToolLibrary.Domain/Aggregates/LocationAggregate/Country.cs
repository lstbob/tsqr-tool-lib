namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

/// <summary>
/// Represents a country in the tool library system.
/// </summary>
public class Country : Entity<CountryId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// </summary>
    private Country(CountryId id, string name) : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Country name is invalid.", nameof(name))
            : name;
    }

    public string Name { get; } 
}
    
