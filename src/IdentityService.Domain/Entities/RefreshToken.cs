namespace IdentityService.Domain.Entities;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public RefreshToken(string token, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        Token = token;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsActive(DateTime utcNow) => !IsRevoked && ExpiresAtUtc > utcNow;

    public void Revoke(DateTime revokedAtUtc)
    {
        RevokedAtUtc ??= revokedAtUtc;
    }
}