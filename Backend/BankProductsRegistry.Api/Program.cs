using System.Text.Json.Serialization;
using BankProductsRegistry.Api.Configuration;
using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Services;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = MySqlConnectionResolver.ResolveConnectionString(builder.Configuration);
var serverVersion = MySqlConnectionResolver.ResolveServerVersion(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = ValidationMessageTranslator.Translate(context.ModelState);

        return new BadRequestObjectResult(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos no validos",
            Detail = "Revisa los campos enviados e intentalo de nuevo."
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BankProductsDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        serverVersion,
        mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure();
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

app.UseExceptionHandler();

var enableSwagger = app.Configuration.GetValue("Swagger:Enabled", true);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

await EnsureDatabaseAsync(app.Services);

app.Run();

return;

static async Task EnsureDatabaseAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseStartup");
    var dbContext = scope.ServiceProvider.GetRequiredService<BankProductsDbContext>();

    try
    {
        var migrations = dbContext.Database.GetMigrations();
        if (migrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo inicializar la base de datos al arrancar la aplicacion.");
        throw;
    }
}
