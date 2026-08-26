namespace IdentityService.Domain.Entities;

public sealed class Role
{
    private readonly List<Permission> _permissions = [];

    private Role()
    {
    }

    public Role(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<Permission> Permissions => _permissions;

    public void AddPermission(Permission permission)
    {
        if (_permissions.Any(existing => existing.Name.Equals(permission.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _permissions.Add(permission);
    }
}