namespace IdentityService.Domain.Entities;

public sealed class User
{
    private readonly List<Role> _roles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    public User(Guid id, string email, string userName)
    {
        Id = id;
        Email = email;
        UserName = userName;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string UserName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(existing => existing.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _roles.Add(role);
    }

    public void ReplaceRoles(IEnumerable<Role> roles)
    {
        _roles.Clear();
        foreach (var role in roles)
        {
            AssignRole(role);
        }
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void RevokeRefreshTokens(DateTime revokedAtUtc)
    {
        foreach (var refreshToken in _refreshTokens.Where(token => token.IsActive(revokedAtUtc)))
        {
            refreshToken.Revoke(revokedAtUtc);
        }
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
    }

    public RefreshToken? GetRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(existing => existing.Token == token);
    }

    public IEnumerable<string> GetPermissions()
    {
        return _roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}