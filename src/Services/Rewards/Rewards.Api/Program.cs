using Microsoft.EntityFrameworkCore;
using Rewards.Application.Interfaces;
using Rewards.Infrastructure.Persistence;
using Rewards.Infrastructure.Repositories;
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
    c.SwaggerDoc("v1", new() { Title = "Rewards Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<RewardsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP clients
builder.Services.AddHttpClient("PointsService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PointsEngine"] ?? "http://localhost:5003");
});

// Repositories
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IRedemptionRepository, RedemptionRepository>();

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
