using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain;

public interface IToolRepository : IRepository<Tool, ToolId>
{
    Task<List<Tool>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ToolStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

public record ToolStats(List<(int Key, string Label, int Count)> ByType, List<(int Key, string Label, int Count)> ByScarcity)
{
    private static readonly Dictionary<int, string> TypeNames = new()
    {
        { 1, "Hand Tool" }, { 2, "Power Tool" }, { 3, "Gardening Tool" },
        { 4, "Construction Tool" }, { 5, "Specialty Tool" }, { 6, "Other" }
    };

    private static readonly Dictionary<int, string> ScarcityNames = new()
    {
        { 1, "Low" }, { 2, "Medium" }, { 3, "High" }, { 4, "Critical" }
    };

    public static string GetTypeName(int key) => TypeNames.GetValueOrDefault(key, "Unknown");
    public static string GetScarcityName(int key) => ScarcityNames.GetValueOrDefault(key, "Unknown");
}
