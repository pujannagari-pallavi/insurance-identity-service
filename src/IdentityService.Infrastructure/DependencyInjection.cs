using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Services;
using IdentityService.Application.Validation.Auth;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(NormalizePostgresConnectionString(configuration.GetConnectionString("IdentityDatabase"))));

        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();
        services.AddScoped<AuthResponseFactory>();
        services.AddScoped<IDefaultRoleAssigner, DefaultRoleAssigner>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }

    private static string NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'IdentityDatabase' was not found.");
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var connectionUri)
            || (connectionUri.Scheme is not "postgres" and not "postgresql"))
        {
            return connectionString;
        }

        var credentials = connectionUri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(connectionUri.AbsolutePath.Trim('/')))
        {
            throw new InvalidOperationException("The PostgreSQL connection URL must include a username, password, and database name.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = connectionUri.Host,
            Port = connectionUri.IsDefaultPort ? 5432 : connectionUri.Port,
            Database = Uri.UnescapeDataString(connectionUri.AbsolutePath.Trim('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}