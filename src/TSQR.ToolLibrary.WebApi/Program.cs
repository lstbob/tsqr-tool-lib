using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Postgres;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Middleware;
using TSQR.ToolLibrary.WebApi.Queries;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? "tsqr-autheo";
var jwtAudience = jwtSection["Audience"] ?? "tsqr-services";

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

// Deny by default: every endpoint requires an authenticated caller unless it
// is explicitly marked [AllowAnonymous] (the public read/catalog endpoints).
// Previously no endpoint enforced authorization, so the entire API — including
// state-changing commands — was anonymous.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder
    .Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var firstError = ctx.ModelState.FirstOrDefault(kvp => kvp.Value?.Errors.Count > 0);

            var error = firstError.Value?.Errors.FirstOrDefault();
            var code = firstError.Key;
            var message = error?.ErrorMessage ?? "Validation failed.";

            return new BadRequestObjectResult(new ErrorResponse(code, message));
        };
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

builder.Services.AddSingleton<ISqlEntityMapping<InventoryItem>, InventoryItemMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<MaintenanceRecord>, MaintenanceRecordMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Member>, MemberMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Reservation>, ReservationMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Tool>, ToolMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Loan>, LoanMapping>();
builder.Services.AddSingleton<ISqlEntityMapping<Policy>, PolicyMapping>();

builder.Services.AddScoped<
    IRepository<InventoryItem, InventoryItemId>,
    InventoryItemRepository
>();
builder.Services.AddScoped<IRepository<Member, MemberId>, SqlRepository<Member, MemberId>>();
builder.Services.AddScoped<IRepository<Loan, LoanId>, SqlRepository<Loan, LoanId>>();
builder.Services.AddScoped<
    IRepository<Reservation, ReservationId>,
    SqlRepository<Reservation, ReservationId>
>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IRepository<Tool, ToolId>>(sp => sp.GetRequiredService<IToolRepository>());
builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
builder.Services.AddScoped<IPolicyRepository, DapperPolicyRepository>();
builder.Services.AddScoped<IRepository<Policy, PolicyId>>(sp => sp.GetRequiredService<IPolicyRepository>());
builder.Services.AddScoped<IDashboardQueries, DashboardQueries>();

builder.Services.AddScoped<IReservationRepository, DapperReservationRepository>();
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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

// Global exception handler logs unhandled exceptions and returns a structured
// 500 ErrorResponse. In Development, UseDeveloperExceptionPage above takes
// precedence (the middleware's `when (!_env.IsDevelopment())` filter lets the
// developer page render instead).
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
