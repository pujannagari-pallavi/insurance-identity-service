using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IdentityService.Domain.Entities;

namespace IdentityService.API;

public interface IPasswordResetEmailService
{
    Task SendAsync(string email, string userName, string rawToken, bool isInvitation, CancellationToken cancellationToken);
}

public sealed class PasswordResetEmailService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<PasswordResetEmailService> logger) : IPasswordResetEmailService
{
    public async Task SendAsync(string email, string userName, string rawToken, bool isInvitation, CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["FRONTEND_URL"] ?? throw new InvalidOperationException("FRONTEND_URL is not configured.");
        var apiKey = configuration["RESEND_API_KEY"] ?? throw new InvalidOperationException("RESEND_API_KEY is not configured.");
        var fromAddress = configuration["EMAIL_FROM_ADDRESS"] ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not configured.");
        var link = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        var subject = isInvitation ? "Set up your SureCover account" : "Reset your SureCover password";
        var body = $"Hello {System.Net.WebUtility.HtmlEncode(userName)},<br><br>{(isInvitation ? "Set your password" : "Reset your password")} using this link. It expires in 60 minutes.<br><br><a href=\"{link}\">{link}</a>";
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = fromAddress,
                to = new[] { email },
                subject,
                html = body
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await httpClientFactory.CreateClient("resend").SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Resend rejected password email to {Email}. Status {StatusCode}: {ResponseBody}", email, (int)response.StatusCode, responseBody);
            throw new InvalidOperationException("The password email could not be sent. Check the Identity Service Resend configuration.");
        }
    }
}

public static class PasswordResetTokenFactory
{
    public static (PasswordResetToken Token, string RawToken) Create(Guid userId)
    {
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        return (new PasswordResetToken(userId, hash, DateTime.UtcNow.AddHours(1)), rawToken);
    }

    public static string Hash(string rawToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}