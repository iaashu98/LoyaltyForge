using Microsoft.EntityFrameworkCore;
using PointsEngine.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Points Engine Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<PointsEngineDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
// TODO: Add repository implementations
// builder.Services.AddScoped<IPointsLedgerRepository, PointsLedgerRepository>();
// builder.Services.AddScoped<IPointsBalanceRepository, PointsBalanceRepository>();
// builder.Services.AddScoped<IRuleRepository, RuleRepository>();

// Rule Engine
// TODO: Add IRuleEngine implementation (to be written by human)

// Event consumers
// TODO: Add RabbitMQ event consumer for OrderPlaced events

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
