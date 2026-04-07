using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using BankProductsRegistry.Api.Configuration;
using BankProductsRegistry.Api.Configuration.Security;
using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Auth;
using BankProductsRegistry.Api.Services;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Swagger;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

DotEnvLoader.LoadIfExists();
QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = MySqlConnectionResolver.ResolveConnectionString(builder.Configuration);
var serverVersion = MySqlConnectionResolver.ResolveServerVersion(builder.Configuration);
var jwtOptionsSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtOptionsSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Authentication:Jwt:Key debe estar configurada con al menos 32 caracteres.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddProblemDetails();
builder.Services.Configure<JwtOptions>(jwtOptionsSection);
builder.Services.AddControllers(options =>
    {
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
    })
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bank Products Registry API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Usa el formato: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    options.OperationFilter<JsonOnlyOperationFilter>();
    options.OperationFilter<DefaultProblemResponsesOperationFilter>();
});
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
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole<int>>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddEntityFrameworkStores<BankProductsDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var securityStamp = context.Principal?.FindFirstValue(CustomClaimTypes.SecurityStamp);

                if (string.IsNullOrWhiteSpace(userIdValue) || string.IsNullOrWhiteSpace(securityStamp))
                {
                    context.Fail("Token invalido.");
                    return;
                }

                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(userIdValue);

                if (user is null || !user.IsActive)
                {
                    context.Fail("Usuario invalido o inactivo.");
                    return;
                }

                if (!string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal))
                {
                    context.Fail("La sesion ya no es valida.");
                    return;
                }

                // Alinear client_id con la BD en cada request (el JWT puede estar desactualizado
                // si se vinculó ClientId después del login sin reemitir token).
                if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                {
                    foreach (var obsolete in claimsIdentity.FindAll(CustomClaimTypes.ClientId).ToList())
                    {
                        claimsIdentity.RemoveClaim(obsolete);
                    }

                    if (user.ClientId.HasValue)
                    {
                        claimsIdentity.AddClaim(new Claim(
                            CustomClaimTypes.ClientId,
                            user.ClientId.Value.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        };
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(AuthPolicies.WriteAccess, policy =>
        policy.RequireRole(AuthRoles.Admin, AuthRoles.Operator))
    .AddPolicy(AuthPolicies.AdminOnly, policy =>
        policy.RequireRole(AuthRoles.Admin));
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddSingleton<IReportPdfService, ReportPdfService>();
builder.Services.AddScoped<IAccountProductBlockService, AccountProductBlockService>();
builder.Services.AddScoped<IAccountProductLimitService, AccountProductLimitService>();
builder.Services.AddScoped<IAccountProductTravelNoticeService, AccountProductTravelNoticeService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseExceptionHandler();

var enableSwagger = app.Environment.IsDevelopment() || app.Configuration.GetValue("Swagger:Enabled", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await EnsureDatabaseAsync(app.Services, app.Configuration);

app.Run();

return;

static async Task EnsureDatabaseAsync(IServiceProvider services, IConfiguration configuration)
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

        await BankProductsDbSeeder.SeedAsync(dbContext);
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await BankIdentitySeeder.SeedAsync(roleManager, userManager, configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo inicializar la base de datos al arrancar la aplicacion.");
        throw;
    }
}
