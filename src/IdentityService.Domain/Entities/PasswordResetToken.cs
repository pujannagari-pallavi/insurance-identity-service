namespace IdentityService.Domain.Entities;

public sealed class PasswordResetToken
{
    private PasswordResetToken()
    {
    }

    public PasswordResetToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public bool IsUsable(DateTime nowUtc) => UsedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Use(DateTime usedAtUtc) => UsedAtUtc = usedAtUtc;
}