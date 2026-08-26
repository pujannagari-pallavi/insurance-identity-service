using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public interface IDefaultRoleAssigner
{
    Task AssignAsync(User user, CancellationToken cancellationToken = default);
}