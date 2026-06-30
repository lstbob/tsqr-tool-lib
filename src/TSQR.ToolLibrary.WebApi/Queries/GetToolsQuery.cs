using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetToolsQuery(string? Q, int? Type, int? ManufacturerId, int Page, int PageSize);

public sealed class GetToolsHandler(IToolRepository toolRepo)
    : IInteractor<GetToolsQuery, PagedResult<ToolListItem>>
{
    public async Task<PagedResult<ToolListItem>> ExecuteAsync(
        GetToolsQuery request,
        CancellationToken ct
    )
    {
        var allTools = await toolRepo.GetAllAsync(ct);

        var filtered = allTools.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.ToLowerInvariant();
            filtered = filtered.Where(t =>
                t.Model.Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
            );
        }
        if (request.Type.HasValue)
            filtered = filtered.Where(t => (int)t.Type == request.Type.Value);
        if (request.ManufacturerId.HasValue)
            filtered = filtered.Where(t => t.Manufacturer.Id.Value == request.ManufacturerId.Value);

        var list = filtered.ToList();
        var totalCount = list.Count;

        // Defensive clamp: never let unvalidated paging params produce a
        // negative Skip (throws) or an integer overflow in (page-1)*pageSize.
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var paged = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = paged
            .Select(t => new ToolListItem(
                t.Id.Value,
                t.Model,
                t.Description,
                (int)t.Type,
                ToolStats.GetTypeName((int)t.Type),
                (int)t.AmortizationRate,
                AmortizationRateName((int)t.AmortizationRate),
                t.Manufacturer.Id.Value,
                t.Manufacturer.Name
            ))
            .ToList();

        return new PagedResult<ToolListItem>(items, totalCount, page, pageSize);
    }

    private static string AmortizationRateName(int rate) =>
        rate switch
        {
            1 => "Low",
            2 => "Medium",
            3 => "High",
            _ => "Unknown",
        };
}
