using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IdentityService.API.Infrastructure;
using IdentityService.Application.Services;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
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
            []
        }
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<IdentityExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAsync();

    var bootstrapAdmin = app.Configuration.GetSection("BootstrapAdmin");
    var bootstrapEmail = bootstrapAdmin["Email"];
    var bootstrapUserName = bootstrapAdmin["UserName"];
    var bootstrapPassword = bootstrapAdmin["Password"];
    var bootstrapValues = new[] { bootstrapEmail, bootstrapUserName, bootstrapPassword };

    if (bootstrapValues.Any(value => !string.IsNullOrWhiteSpace(value)))
    {
        if (bootstrapValues.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin requires Email, UserName, and Password when it is configured.");
        }

        using var scope = app.Services.CreateScope();
        var bootstrapService = scope.ServiceProvider.GetRequiredService<BootstrapAdminService>();
        var wasCreated = await bootstrapService.EnsureCreatedAsync(
            bootstrapEmail!,
            bootstrapUserName!,
            bootstrapPassword!);

        if (wasCreated)
        {
            app.Logger.LogInformation("Created the configured bootstrap PlatformAdmin user.");
        }
    }
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new
{
    service = "identity-service",
    status = "healthy",
    timestampUtc = DateTime.UtcNow
}));

app.Run();

public partial class Program;
