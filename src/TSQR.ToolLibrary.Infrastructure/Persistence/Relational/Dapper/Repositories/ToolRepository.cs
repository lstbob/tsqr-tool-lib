using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class ToolRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Tool> mapping,
    ISqlDialect dialect,
    IDomainEventCollector eventCollector)
    : SqlRepository<Tool, ToolId>(uow, mapping, dialect, eventCollector),
        IToolRepository
{
    public override async Task AddAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(entity, cancellationToken);

        foreach (var kvp in entity.ScarcityByLocation)
            await Database.ExecuteAsync(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new
                {
                    ToolId = entity.Id.Value,
                    LocationId = kvp.Key.Value,
                    ScarcityLevel = kvp.Value,
                }
            );
    }

    public override void Update(Tool entity)
    {
        base.Update(entity);

        Database.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value }
        );

        foreach (var kvp in entity.ScarcityByLocation)
            Database.Execute(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new
                {
                    ToolId = entity.Id.Value,
                    LocationId = kvp.Key.Value,
                    ScarcityLevel = kvp.Value,
                }
            );
    }

    public override void Delete(Tool entity)
    {
        Database.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value }
        );

        base.Delete(entity);
    }
}
