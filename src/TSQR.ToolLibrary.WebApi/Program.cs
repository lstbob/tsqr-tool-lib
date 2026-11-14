using TSQR.Common.Messaging;
using TSQR.Common.WebApi;
using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Outbox;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Postgres;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Messaging;
using TSQR.ToolLibrary.WebApi.Queries;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.AddTsqrApiDefaults(options =>
{
    options.ServiceName = "tsqr-tool-lib";
});

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));
builder.Services.AddSingleton<ISqlDialect>(_ => new PostgresDialect());

builder.Services.AddScoped<ISqlUnitOfWork>(sp =>
    new DapperUnitOfWork(
        sp.GetRequiredService<IDbConnectionFactory>(),
        sp.GetRequiredService<ISqlDialect>()));

// The application layer depends only on IUnitOfWork (from the Domain). Register
// the same Dapper adapter against IUnitOfWork so DomainEventOrchestrator can
// resolve it without ever referencing a SQL/Dapper type.
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ISqlUnitOfWork>());

builder.Services.AddSingleton<ISqlEntityMapping<InventoryItem, InventoryItemId>, InventoryItemMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<MaintenanceRecord, MaintenanceRecordId>, MaintenanceRecordMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Member, MemberId>, MemberMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Reservation, ReservationId>, ReservationMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Tool, ToolId>, ToolMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Loan, LoanId>, LoanMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Policy, PolicyId>, PolicyMapping>();

builder.Services.AddScoped<
    IRepository<InventoryItem, InventoryItemId>,
    InventoryItemRepository
>();
builder.Services.AddScoped<IRepository<Member, MemberId>, SqlRepository<Member, MemberId>>();
builder.Services.AddScoped<ILoanRepository, DapperLoanRepository>();
builder.Services.AddScoped<IRepository<Loan, LoanId>>(sp => sp.GetRequiredService<ILoanRepository>());
builder.Services.AddScoped<
    IRepository<Reservation, ReservationId>,
    SqlRepository<Reservation, ReservationId>
>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IRepository<Tool, ToolId>>(sp => sp.GetRequiredService<IToolRepository>());
builder.Services.AddScoped<IToolQueries, ToolQueries>();
builder.Services.AddScoped<IManufacturerLookup, ManufacturerLookup>();
builder.Services.AddScoped<IPolicyRepository, DapperPolicyRepository>();
builder.Services.AddScoped<IRepository<Policy, PolicyId>>(sp => sp.GetRequiredService<IPolicyRepository>());
builder.Services.AddScoped<IDashboardQueries, DashboardQueries>();

builder.Services.AddScoped<IReservationRepository, DapperReservationRepository>();
builder.Services.AddScoped<IOutboxStore, DapperOutboxStore>();
builder.Services.AddScoped<ICrossDomainEventBus, OutboxCrossDomainEventBus>();
builder.Services.AddScoped<IIntegrationEventTransport, LoggingIntegrationEventTransport>();
builder.Services.AddHostedService<OutboxPublisherService>();
builder.Services.AddApplicationFeatures();

builder.Services.AddScoped<
    IInteractor<GetToolsQuery, PagedResult<ToolListItem>>,
    GetToolsHandler
>();
builder.Services.AddScoped<IInteractor<GetToolByIdQuery, ToolDetail?>, GetToolByIdHandler>();
builder.Services.AddScoped<IInteractor<GetToolStatsQuery, ToolStatsResult>, GetToolStatsHandler>();
builder.Services.AddScoped<
    IInteractor<GetDashboardStatsQuery, DashboardStats>,
    GetDashboardStatsHandler
>();
builder.Services.AddScoped<
    IInteractor<GetManufacturersQuery, List<ManufacturerDto>>,
    GetManufacturersHandler
    >();
builder.Services.AddScoped<IInteractor<GetMembersQuery, PagedResult<MemberListItem>>, GetMembersHandler>();
builder.Services.AddScoped<IInteractor<GetMemberByIdQuery, MemberDetail?>, GetMemberByIdHandler>();
builder.Services.AddScoped<IInteractor<GetReservationsQuery, PagedResult<ReservationListItem>>, GetReservationsHandler>();
builder.Services.AddScoped<IInteractor<GetReservationByIdQuery, ReservationListItem?>, GetReservationByIdHandler>();
builder.Services.AddScoped<IInteractor<GetInventoryQuery, PagedResult<InventoryListItem>>, GetInventoryHandler>();
builder.Services.AddScoped<IInteractor<GetInventoryByIdQuery, InventoryListItem?>, GetInventoryByIdHandler>();
builder.Services.AddScoped<IInteractor<GetMaintenanceQuery, PagedResult<MaintenanceListItem>>, GetMaintenanceHandler>();

var app = builder.Build();

app.UseTsqrApiDefaults();
app.Run();
