using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Domain.Entities;

namespace IdentityService.API;

public interface IPasswordResetEmailService
{
    Task SendAsync(string email, string userName, string rawToken, bool isInvitation, CancellationToken cancellationToken);
}

public sealed class PasswordResetEmailService(IConfiguration configuration, ILogger<PasswordResetEmailService> logger) : IPasswordResetEmailService
{
    public async Task SendAsync(string email, string userName, string rawToken, bool isInvitation, CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["FRONTEND_URL"] ?? throw new InvalidOperationException("FRONTEND_URL is not configured.");
        var link = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        var subject = isInvitation ? "Set up your SureCover account" : "Reset your SureCover password";
        var body = $"Hello {WebUtility.HtmlEncode(userName)},<br><br>{(isInvitation ? "Set your password" : "Reset your password")} using this link. It expires in 60 minutes.<br><br><a href=\"{link}\">{link}</a>";
        var host = configuration["SMTP_HOST"];
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("SMTP is not configured. Password link for {Email}: {Link}", email, link);
            return;
        }

        using var client = new SmtpClient(host, int.TryParse(configuration["SMTP_PORT"], out var port) ? port : 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(configuration["SMTP_USERNAME"], configuration["SMTP_PASSWORD"])
        };
        using var message = new MailMessage(configuration["SMTP_FROM_ADDRESS"] ?? configuration["SMTP_USERNAME"]!, email, subject, body) { IsBodyHtml = true };
        await client.SendMailAsync(message, cancellationToken);
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