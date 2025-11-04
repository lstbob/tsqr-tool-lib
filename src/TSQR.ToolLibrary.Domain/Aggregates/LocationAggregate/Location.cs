namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

/// <summary>
/// Represents a location in the tool library system.
/// </summary>
public class Location : Entity<LocationId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    private Location(LocationId id, string name, Country country, Address address)
        : base(id)
    {
        Name = name.Validate(nameof(name));
        Country = country ?? throw new ArgumentNullException(nameof(country));
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    /// <summary>
    /// Responsible for rehydrating existing instances of <see cref="Location"/>
    /// </summary>
    /// <param name="id">Identifier of the location</param>
    /// <param name="name">Name of the location</param>
    /// <param name="country">Country of the location</param>
    /// <param name="address">Address of the location</param>
    /// <exception cref="ArgumentNullException">Throws exception if invalid arguments are passed</exception>
    /// <returns>New instance of a location.</returns>
    internal static Location Create(LocationId id, string name, string country, string address)
    {
        return new(id, name, Country.Create(country), new Address(address));
    }

    /// <summary>
    /// Responsible for creating new instances of <see cref="Location" />
    /// </summary>
    /// <param name="name">Name of the location</param>
    /// <param name="country">Country of the location</param>
    /// <param name="address">Address of the location</param>
    /// <exception cref="ArgumentNullException">Throws exception if invalid arguments are passed</exception>
    /// <returns>*.</returns>
    internal static Location Creates(string name, string country, string address)
    {
        return new(new LocationId(default), name, Country.Create(country), new Address(address));
    }

    public string Name { get; private set; }
    public Country Country { get; }
    public Address Address { get; private set; }

    /// <summary>
    /// Responsible for changing an existing address for a location.
    /// </summary>
    /// <param name="newAddress">The new address of the location</param>
    /// <exception cref="ArgumentException">Throws exception if new address is invalid</exception>
    public void ChangeAddress(string newAddress)
    {
        Address = new Address(newAddress.Validate(nameof(newAddress)));
    }

    /// <summary>
    /// Responsible for changing the name of the location.
    /// </summary>
    /// <param name="newName">The new name of the location.</param>
    /// <exception cref="ArgumentException">Throws exception if new name is invalid</exception>
    public void ChangeName(string newName)
    {
        Name = newName.Validate(nameof(newName));
    }
}
