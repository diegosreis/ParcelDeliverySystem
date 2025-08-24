using System.Reflection;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Parcel Delivery System API",
        Version = "v1.0.0",
        Description = "A robust and scalable system for automating internal parcel handling in distribution centers. " +
                      "This system processes parcels based on configurable business rules for weight and value thresholds.",
        Contact = new OpenApiContact
        {
            Name = "Diego Reis",
            Email = "diegoreis@gmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// Register repositories with proper lifetime management
builder.Services.AddSingleton<IShippingContainerRepository, ShippingContainerRepository>();
builder.Services.AddSingleton<IParcelRepository, ParcelRepository>();
builder.Services.AddSingleton<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddSingleton<IBusinessRuleRepository, BusinessRuleRepository>();

// Register application services
builder.Services.AddScoped<IXmlImportService, XmlImportService>();
builder.Services.AddScoped<IDepartmentRuleService, DepartmentRuleService>();
builder.Services.AddScoped<IParcelProcessingService, ParcelProcessingService>();
builder.Services.AddScoped<IBusinessRuleService, BusinessRuleService>();
builder.Services.AddScoped<IDataInitializationService, DataInitializationService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());


// Configure CORS for development
if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parcel Delivery System API v1.0.0");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Initialize application data on startup
using (var scope = app.Services.CreateScope())
{
    var dataInitializationService = scope.ServiceProvider.GetRequiredService<IDataInitializationService>();
    await dataInitializationService.InitializeAsync();
}

app.Run();