using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;
using TSQR.ToolLibrary.Application.Tool.Commands;
using TSQR.ToolLibrary.Infrastructure.Dapper;
using TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped(_ => new DapperUnitOfWork(connectionString));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegisterToolCommand).Assembly));

builder.Services.AddScoped<IRepository<Tool, ToolId>, ToolRepository>();
builder.Services.AddScoped<IRepository<InventoryItem, InventoryItemId>, InventoryItemRepository>();
builder.Services.AddScoped<IRepository<Member, MemberId>, MemberRepository>();
builder.Services.AddScoped<IRepository<Reservation, ReservationId>, ReservationRepository>();
builder.Services.AddScoped<IRepository<MaintenanceRecord, MaintenanceRecordId>, MaintenanceRecordRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
