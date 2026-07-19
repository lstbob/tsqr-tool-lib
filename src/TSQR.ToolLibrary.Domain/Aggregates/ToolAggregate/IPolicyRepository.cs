using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Repository for <see cref="Policy"/> aggregates. Lookup is keyed on
/// <see cref="ToolType"/> with an optional <see cref="LocationId"/>:
/// an exact (ToolType, LocationId) match wins; otherwise the global
/// (ToolType, LocationId=null) policy is used.
/// </summary>
public interface IPolicyRepository : IRepository<Policy, PolicyId>
{
    /// <summary>
    /// Returns the best-matching policy for a given tool type and (optional)
    /// location. Falls back to the global (LocationId=null) policy for the
    /// tool type if no location-specific one exists.
    /// </summary>
    Task<Policy?> GetByToolTypeAsync(
        ToolType toolType,
        LocationId? locationId,
        CancellationToken cancellationToken = default);
}