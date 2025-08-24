using System.Reflection;
using System.Text.Json.Serialization;
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

// Add services to the container with enum string conversion
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure Swagger/OpenAPI with improved settings
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

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    // Add additional XML files for referenced projects
    const string applicationXmlFile = "Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath)) c.IncludeXmlComments(applicationXmlPath);

    const string domainXmlFile = "Domain.xml";
    var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXmlFile);
    if (File.Exists(domainXmlPath)) c.IncludeXmlComments(domainXmlPath);
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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parcel Delivery System API v1.0.0");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    c.DocumentTitle = "Parcel Delivery System API Documentation";
    c.DefaultModelsExpandDepth(1); // Show first level of models
});

// Use CORS before other middleware
app.UseCors("AllowAll");

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