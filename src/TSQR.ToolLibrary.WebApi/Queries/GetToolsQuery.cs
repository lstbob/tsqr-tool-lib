using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetToolsQuery(string? Q, int? Type, int? ManufacturerId, int Page, int PageSize);

public sealed class GetToolsHandler : IInteractor<GetToolsQuery, PagedResult<ToolListItem>>
{
    private readonly IToolRepository _toolRepo;
    private readonly IManufacturerRepository _manufacturerRepo;

    public GetToolsHandler(IToolRepository toolRepo, IManufacturerRepository manufacturerRepo)
    {
        _toolRepo = toolRepo;
        _manufacturerRepo = manufacturerRepo;
    }

    public async Task<PagedResult<ToolListItem>> ExecuteAsync(GetToolsQuery request, CancellationToken ct)
    {
        var allTools = await _toolRepo.GetAllAsync(ct);

        var filtered = allTools.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.ToLowerInvariant();
            filtered = filtered.Where(t =>
                t.Model.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        if (request.Type.HasValue)
            filtered = filtered.Where(t => (int)t.Type == request.Type.Value);
        if (request.ManufacturerId.HasValue)
            filtered = filtered.Where(t => t.Manufacturer.Id.Value == request.ManufacturerId.Value);

        var list = filtered.ToList();
        var totalCount = list.Count;

        var paged = list
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = paged.Select(t => new ToolListItem(
            t.Id.Value,
            t.Model,
            t.Description,
            (int)t.Type,
            ToolStats.GetTypeName((int)t.Type),
            (int)t.AmortizationRate,
            AmortizationRateName((int)t.AmortizationRate),
            t.Manufacturer.Id.Value,
            t.Manufacturer.Name)).ToList();

        return new PagedResult<ToolListItem>(items, totalCount, request.Page, request.PageSize);
    }

    private static string AmortizationRateName(int rate) => rate switch
    {
        1 => "Low", 2 => "Medium", 3 => "High", _ => "Unknown"
    };
}
