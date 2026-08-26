using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityService.API.Tests;

public sealed class IdentityApiFactory(
    IRegistrationService registrationService,
    ILoginService loginService,
    IRefreshTokenService refreshTokenService) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "IdentityService.API")));
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRegistrationService>();
            services.RemoveAll<ILoginService>();
            services.RemoveAll<IRefreshTokenService>();

            services.AddSingleton(registrationService);
            services.AddSingleton(loginService);
            services.AddSingleton(refreshTokenService);
        });
    }
}