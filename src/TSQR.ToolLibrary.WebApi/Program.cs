using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Application.Tool.Commands;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.Application.Reservation.Commands;
using TSQR.ToolLibrary.Application.Inventory.Commands;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped(typeof(IRepository<,>), typeof(InMemoryRepository<,>));
builder.Services.AddScoped<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<IUnitOfWork>(_ => new InMemoryUnitOfWork());
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
