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
        Name = name.Validate(nameof(name));
    }
    
    /// <summary>
    /// Static factory method to create a new <see cref="Country"/> instance. 
    /// </summary>
    public static Country Create(string name)
    {
        return new (new (default), name);
    }

    /// <summary>
    /// Static factory method to rehydrate a <see cref="Country"/> instance. 
    /// </summary>
    public static Country Create(CountryId id, string name)
    {
        return new (id, name);
    }
    
    public string Name { get; } 
}
    
