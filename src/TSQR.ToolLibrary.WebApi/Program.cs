using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;
using TSQR.ToolLibrary.Application.Tool.Commands;
using TSQR.ToolLibrary.Infrastructure.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Dapper;
using TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;
using TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<IDatabaseUnitOfWork>(_ => new DapperUnitOfWork(connectionString));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegisterToolCommand).Assembly));

builder.Services.AddSingleton<IEntityMapping<InventoryItem>, InventoryItemMapping>();
builder.Services.AddSingleton<IEntityMapping<Member>, MemberMapping>();
builder.Services.AddSingleton<IEntityMapping<Reservation>, ReservationMapping>();
builder.Services.AddSingleton<IEntityMapping<MaintenanceRecord>, MaintenanceRecordMapping>();
builder.Services.AddSingleton<IEntityMapping<Tool>, ToolMapping>();

builder.Services.AddScoped<IRepository<InventoryItem, InventoryItemId>, Repository<InventoryItem, InventoryItemId>>();
builder.Services.AddScoped<IRepository<Member, MemberId>, Repository<Member, MemberId>>();
builder.Services.AddScoped<IRepository<Reservation, ReservationId>, Repository<Reservation, ReservationId>>();
builder.Services.AddScoped<IRepository<MaintenanceRecord, MaintenanceRecordId>, Repository<MaintenanceRecord, MaintenanceRecordId>>();
builder.Services.AddScoped<IRepository<Tool, ToolId>, ToolRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
