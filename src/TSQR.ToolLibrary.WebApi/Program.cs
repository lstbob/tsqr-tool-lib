using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Infrastructure.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Dapper;
using TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;
using TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? "tsqr-identity";
var jwtAudience = jwtSection["Audience"] ?? "tsqr-services";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var firstError = ctx.ModelState
                .FirstOrDefault(kvp => kvp.Value?.Errors.Count > 0);

            var error = firstError.Value?.Errors.FirstOrDefault();
            var code = firstError.Key;
            var message = error?.ErrorMessage ?? "Validation failed.";

            return new BadRequestObjectResult(new ErrorResponse(code, message));
        };
    });

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
builder.Services.AddApplicationFeatures();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
