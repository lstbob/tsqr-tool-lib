namespace TSQR.ToolLibrary.WebApi.Controllers.Dtos;

public record ToolListItem(
    int Id,
    string Model,
    string Description,
    int ToolType,
    string ToolTypeName,
    int AmortizationRate,
    string AmortizationRateName,
    int ManufacturerId,
    string ManufacturerName);

public record ToolDetail(
    int Id,
    string Model,
    string Description,
    int ManufacturerId,
    string ManufacturerName,
    int ToolType,
    string ToolTypeName,
    int AmortizationRate,
    string AmortizationRateName,
    string? Metadata,
    List<ScarcityDto> ScarcityByLocation);

public record ScarcityDto(int LocationId, string LocationName, int ScarcityLevel, string ScarcityLevelName);

public record ToolStatsItem(int Key, string Label, int Count);

public record ManufacturerDto(int Id, string Name);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
