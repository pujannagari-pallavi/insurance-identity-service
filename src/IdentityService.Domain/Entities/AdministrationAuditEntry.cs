namespace IdentityService.Domain.Entities;

public sealed class AdministrationAuditEntry
{
    private AdministrationAuditEntry()
    {
    }

    public AdministrationAuditEntry(Guid actorUserId, Guid targetUserId, string action, string details)
    {
        Id = Guid.NewGuid();
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        Action = action;
        Details = details;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }
}