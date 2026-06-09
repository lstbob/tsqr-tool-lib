using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetToolByIdQuery(int Id);

public sealed class GetToolByIdHandler : IInteractor<GetToolByIdQuery, ToolDetail?>
{
    private readonly IToolRepository _toolRepo;

    public GetToolByIdHandler(IToolRepository toolRepo)
    {
        _toolRepo = toolRepo;
    }

    public async Task<ToolDetail?> ExecuteAsync(GetToolByIdQuery request, CancellationToken ct)
    {
        var tool = await _toolRepo.GetByIdAsync(new ToolId(request.Id), ct);
        if (tool is null) return null;

        var scarcity = tool.ScarcityByLocation.Select(kvp => new ScarcityDto(
            kvp.Key.Value,
            LocationName((int)kvp.Key.Value),
            (int)kvp.Value,
            ScarcityLevelName((int)kvp.Value))).ToList();

        return new ToolDetail(
            tool.Id.Value,
            tool.Model,
            tool.Description,
            tool.Manufacturer.Id.Value,
            tool.Manufacturer.Name,
            (int)tool.Type,
            ToolStats.GetTypeName((int)tool.Type),
            (int)tool.AmortizationRate,
            AmortizationRateName((int)tool.AmortizationRate),
            tool.Metadata,
            scarcity);
    }

    private static string AmortizationRateName(int rate) => rate switch
    {
        1 => "Low", 2 => "Medium", 3 => "High", _ => "Unknown"
    };

    private static string ScarcityLevelName(int level) => level switch
    {
        1 => "Low", 2 => "Medium", 3 => "High", 4 => "Critical", _ => "Unknown"
    };

    private static string LocationName(int locationId) => locationId switch
    {
        1 => "Downtown Workshop",
        2 => "North Side Branch",
        3 => "East End Hub",
        4 => "Westside Station",
        5 => "Central Warehouse",
        _ => $"Location {locationId}"
    };
}
