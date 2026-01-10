using EcommerceIntegration.Application.Interfaces;
using EcommerceIntegration.Application.DTOs.Shopify;
using EcommerceIntegration.Infrastructure.Shopify;
using EcommerceIntegration.Infrastructure.Persistence;
using LoyaltyForge.Messaging.RabbitMQ;
using Microsoft.EntityFrameworkCore;
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
    c.SwaggerDoc("v1", new() { Title = "E-commerce Integration Service", Version = "v1" });
});

// Shopify services
builder.Services.AddScoped<IWebhookSignatureValidator, ShopifySignatureValidator>();
builder.Services.AddScoped<IEventTransformer<ShopifyOrderPayload>, ShopifyOrderTransformer>();

// Database
builder.Services.AddDbContext<IntegrationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Outbox Repository
builder.Services.AddScoped<LoyaltyForge.Common.Outbox.IOutboxRepository, EcommerceIntegration.Infrastructure.Repositories.OutboxRepository>();

// RabbitMQ Configuration
builder.Services.Configure<LoyaltyForge.Messaging.RabbitMQ.RabbitMQOptions>(
    builder.Configuration.GetSection("RabbitMQ"));

// RabbitMQ Event Publisher
builder.Services.AddRabbitMQEventPublisher();

// Outbox Publisher Background Service
builder.Services.AddHostedService<LoyaltyForge.Messaging.Outbox.OutboxPublisher>();

// Health checks
builder.Services.AddHealthChecks();

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
