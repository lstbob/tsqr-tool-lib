namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Immutable reference to a manufacturer catalog entry. A manufacturer is
/// selected when a tool is registered or updated; it has no independent
/// lifecycle inside the tool aggregate.
/// </summary>
public class Manufacturer : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manufacturer"/> class.
    /// </summary>
    private Manufacturer(ManufacturerId id, string name, int communityId = 0)
    {
        Id = id;
        Name = name;
        CommunityId = communityId;
    }

    public ManufacturerId Id { get; }
    public string Name { get; }
    public int CommunityId { get; }

    public static Manufacturer Create(ManufacturerId id, string name, int communityId = 0)
    {
        return new(id, name, communityId);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Name;
        yield return CommunityId;
    }
}
