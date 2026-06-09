using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Application.Tool.Commands;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.Application.Reservation.Commands;
using TSQR.ToolLibrary.Application.Inventory.Commands;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Application.Events;
using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;
using TSQR.ToolLibrary.Infrastructure.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Dapper;
using TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;
using TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;
using TSQR.ToolLibrary.WebApi.Queries;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<IDatabaseUnitOfWork>(_ => new DapperUnitOfWork(connectionString));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetToolsQuery).Assembly);
});

builder.Services.AddSingleton<IEntityMapping<InventoryItem>, InventoryItemMapping>();
builder.Services.AddSingleton<IEntityMapping<Member>, MemberMapping>();
builder.Services.AddSingleton<IEntityMapping<Reservation>, ReservationMapping>();
builder.Services.AddSingleton<IEntityMapping<MaintenanceRecord>, MaintenanceRecordMapping>();
builder.Services.AddSingleton<IEntityMapping<Tool>, ToolMapping>();

builder.Services.AddScoped<IRepository<InventoryItem, InventoryItemId>, Repository<InventoryItem, InventoryItemId>>();
builder.Services.AddScoped<IRepository<Member, MemberId>, Repository<Member, MemberId>>();
builder.Services.AddScoped<IRepository<Reservation, ReservationId>, Repository<Reservation, ReservationId>>();
builder.Services.AddScoped<IRepository<MaintenanceRecord, MaintenanceRecordId>, Repository<MaintenanceRecord, MaintenanceRecordId>>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
builder.Services.AddScoped<IDashboardQueries, DashboardQueries>();

builder.Services.AddScoped<IReservationRepository, DapperReservationRepository>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

builder.Services.AddScoped<IInteractor<RegisterToolCommand, Result<ToolId>>, RegisterToolCommandHandler>();
builder.Services.AddScoped<IInteractor<VerifyMemberCommand, Result>, VerifyMemberCommandHandler>();
builder.Services.AddScoped<IInteractor<RequestMemberAccessCommand, Result>, RequestMemberAccessCommandHandler>();
builder.Services.AddScoped<IInteractor<ApproveMemberAccessCommand, Result>, ApproveMemberAccessCommandHandler>();
builder.Services.AddScoped<IInteractor<ReserveToolCommand, Result<ReservationId>>, ReserveToolCommandHandler>();
builder.Services.AddScoped<IInteractor<CancelReservationCommand, Result>, CancelReservationCommandHandler>();
builder.Services.AddScoped<IInteractor<ConfirmPickupCommand, Result>, ConfirmPickupCommandHandler>();
builder.Services.AddScoped<IInteractor<ReturnToolCommand, Result>, ReturnToolCommandHandler>();
builder.Services.AddScoped<IInteractor<MarkToolForRepairCommand, Result>, MarkToolForRepairCommandHandler>();
builder.Services.AddScoped<IInteractor<CompleteRepairCommand, Result>, CompleteRepairCommandHandler>();
builder.Services.AddScoped<IInteractor<LoanToolCommand, Result>, LoanToolCommandHandler>();
builder.Services.AddScoped<IInteractor<MarkLoanAsNotReturnedCommand, Result>, MarkLoanAsNotReturnedCommandHandler>();

builder.Services.AddScoped<IDomainEventHandler<ReservationCancelledEvent>, ReservationCancelledEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<ToolReturnedEvent>, ToolReturnedEventHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapPost("/tools/register", async (
    RegisterToolCommand command,
    IInteractor<RegisterToolCommand, Result<ToolId>> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});

app.MapPost("/members/request-access", async (
    RequestMemberAccessCommand command,
    IInteractor<RequestMemberAccessCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/members/approve-access", async (
    ApproveMemberAccessCommand command,
    IInteractor<ApproveMemberAccessCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/members/verify", async (
    VerifyMemberCommand command,
    IInteractor<VerifyMemberCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/reservations/create", async (
    ReserveToolCommand command,
    IInteractor<ReserveToolCommand, Result<ReservationId>> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});

app.MapPost("/reservations/cancel", async (
    CancelReservationCommand command,
    IInteractor<CancelReservationCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/reservations/confirm-pickup", async (
    ConfirmPickupCommand command,
    IInteractor<ConfirmPickupCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/tools/loan", async (
    LoanToolCommand command,
    IInteractor<LoanToolCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/tools/return", async (
    ReturnToolCommand command,
    IInteractor<ReturnToolCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/tools/mark-for-repair", async (
    MarkToolForRepairCommand command,
    IInteractor<MarkToolForRepairCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/loans/mark-not-returned", async (
    MarkLoanAsNotReturnedCommand command,
    IInteractor<MarkLoanAsNotReturnedCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.MapPost("/tools/complete-repair", async (
    CompleteRepairCommand command,
    IInteractor<CompleteRepairCommand, Result> handler) =>
{
    var result = await handler.ExecuteAsync(command);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});

app.Run();
