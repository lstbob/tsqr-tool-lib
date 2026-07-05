namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a manufacturer in the tool library system.
/// </summary>
public class Manufacturer : Entity<ManufacturerId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Manufacturer"/> class.
    /// </summary>
    private Manufacturer(ManufacturerId id, string name, int communityId = 0) : base(id)
    {
        Name = name;
        CommunityId = communityId;
    }

    public string Name { get; }
    public int CommunityId { get; private set; }

    public static Manufacturer Create(ManufacturerId id, string name, int communityId = 0)
    {
        return new(id, name, communityId);
    }
}

