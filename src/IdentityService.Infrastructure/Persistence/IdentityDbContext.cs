using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    internal static readonly Guid CustomerRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA001");
    internal static readonly Guid KycReviewerRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA002");
    internal static readonly Guid PolicyUnderwriterRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA003");
    internal static readonly Guid ClaimsAdjusterRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA004");
    internal static readonly Guid PaymentOperationsRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA005");
    internal static readonly Guid SupportAgentRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA006");
    internal static readonly Guid ComplianceOfficerRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA007");
    internal static readonly Guid PlatformAdminRoleId = Guid.Parse("6A34F111-5C2B-4A86-95A9-6D622C2FA008");
    internal static readonly Guid IdentityProfileReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D001");
    internal static readonly Guid IdentityTokenRefreshPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D002");
    internal static readonly Guid KycSubmitPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D003");
    internal static readonly Guid KycVerifyPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D004");
    internal static readonly Guid PolicyReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D005");
    internal static readonly Guid PolicyWritePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D006");
    internal static readonly Guid PolicyWriteAnyPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D007");
    internal static readonly Guid PolicyApprovePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D008");
    internal static readonly Guid ClaimReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D009");
    internal static readonly Guid ClaimDecidePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D010");
    internal static readonly Guid PaymentReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D011");
    internal static readonly Guid PaymentRefundPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D012");
    internal static readonly Guid CustomerReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D013");
    internal static readonly Guid CustomerWritePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D014");
    internal static readonly Guid KycAuditReadPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D015");
    internal static readonly Guid ReportingViewPermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D016");
    internal static readonly Guid IdentityUsersManagePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D017");
    internal static readonly Guid IdentityRolesManagePermissionId = Guid.Parse("D43137C8-2FAF-4B99-B36A-B71D39A9D018");

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AdministrationAuditEntry> AdministrationAuditEntries => Set<AdministrationAuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureAdministrationAuditEntry(modelBuilder);
        SeedIdentityData(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<User>();

        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.Property(user => user.UserName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Metadata.FindNavigation(nameof(User.Roles))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(User.RefreshTokens))?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(user => user.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserRoles",
                right => right.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("UserRoles");
                    join.HasKey("UserId", "RoleId");
                });

        builder
            .HasMany(user => user.RefreshTokens)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Role>();

        builder.ToTable("Roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(100).IsRequired();
        builder.Property(role => role.Description).HasMaxLength(256).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();
        builder.Metadata.FindNavigation(nameof(Role.Permissions))?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                right => right.HasOne<Permission>().WithMany().HasForeignKey("PermissionId").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("RolePermissions");
                    join.HasKey("RoleId", "PermissionId");
                });
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Permission>();

        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Name).HasMaxLength(150).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(256).IsRequired();
        builder.HasIndex(permission => permission.Name).IsUnique();
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<RefreshToken>();

        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.Property(token => token.Token).HasMaxLength(512).IsRequired();
        builder.Property(token => token.CreatedAtUtc).IsRequired();
        builder.Property(token => token.ExpiresAtUtc).IsRequired();
        builder.HasIndex(token => token.Token).IsUnique();
    }

    private static void ConfigureAdministrationAuditEntry(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<AdministrationAuditEntry>();
        builder.ToTable("AdministrationAuditEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Action).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.Details).HasMaxLength(2000).IsRequired();
        builder.Property(entry => entry.OccurredAtUtc).IsRequired();
        builder.HasIndex(entry => entry.TargetUserId);
        builder.HasIndex(entry => entry.OccurredAtUtc);
    }

    private static void SeedIdentityData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new
            {
                Id = CustomerRoleId,
                Name = DefaultRoles.Customer,
                Description = "Default customer role for platform users."
            },
            new
            {
                Id = KycReviewerRoleId,
                Name = DefaultRoles.KycReviewer,
                Description = "Reviews and decides KYC verification cases."
            },
            new
            {
                Id = PolicyUnderwriterRoleId,
                Name = DefaultRoles.PolicyUnderwriter,
                Description = "Reviews and approves insurance policy applications."
            },
            new
            {
                Id = ClaimsAdjusterRoleId,
                Name = DefaultRoles.ClaimsAdjuster,
                Description = "Reviews and decides insurance claim cases."
            },
            new
            {
                Id = PaymentOperationsRoleId,
                Name = DefaultRoles.PaymentOperations,
                Description = "Handles payment operations and refunds."
            },
            new
            {
                Id = SupportAgentRoleId,
                Name = DefaultRoles.SupportAgent,
                Description = "Assists customers with account and policy enquiries."
            },
            new
            {
                Id = ComplianceOfficerRoleId,
                Name = DefaultRoles.ComplianceOfficer,
                Description = "Audits KYC activity and reviews compliance reporting."
            },
            new
            {
                Id = PlatformAdminRoleId,
                Name = DefaultRoles.PlatformAdmin,
                Description = "Manages platform users, roles, and operational access."
            });

        modelBuilder.Entity<Permission>().HasData(
            new
            {
                Id = IdentityProfileReadPermissionId,
                Name = "identity.profile.read",
                Description = "Read own profile."
            },
            new
            {
                Id = IdentityTokenRefreshPermissionId,
                Name = "identity.token.refresh",
                Description = "Refresh authentication token."
            },
            new
            {
                Id = KycSubmitPermissionId,
                Name = "Kyc.Submit",
                Description = "Submit identity documents for KYC verification."
            },
            new
            {
                Id = KycVerifyPermissionId,
                Name = "Kyc.Verify",
                Description = "Review and decide KYC verification cases."
            },
            new { Id = PolicyReadPermissionId, Name = "Policy.Read", Description = "Read policy records." },
            new { Id = PolicyWritePermissionId, Name = "Policy.Write", Description = "Create or update own policy records." },
            new { Id = PolicyWriteAnyPermissionId, Name = "Policy.Write.Any", Description = "Create or update policy records for any customer." },
            new { Id = PolicyApprovePermissionId, Name = "Policy.Approve", Description = "Approve or decline policy applications." },
            new { Id = ClaimReadPermissionId, Name = "Claim.Read", Description = "Read claim records." },
            new { Id = ClaimDecidePermissionId, Name = "Claim.Decide", Description = "Approve or decline claim cases." },
            new { Id = PaymentReadPermissionId, Name = "Payment.Read", Description = "Read payment records." },
            new { Id = PaymentRefundPermissionId, Name = "Payment.Refund", Description = "Process approved payment refunds." },
            new { Id = CustomerReadPermissionId, Name = "Customer.Read", Description = "Read customer records." },
            new { Id = CustomerWritePermissionId, Name = "Customer.Write", Description = "Create or update customer records." });

        modelBuilder.Entity<Permission>().HasData(new { Id = KycAuditReadPermissionId, Name = "Kyc.Audit.Read", Description = "Read KYC audit activity." });
        modelBuilder.Entity<Permission>().HasData(new { Id = ReportingViewPermissionId, Name = "Reporting.View", Description = "View operational and compliance reports." });
        modelBuilder.Entity<Permission>().HasData(new { Id = IdentityUsersManagePermissionId, Name = "Identity.Users.Manage", Description = "Manage platform user access." });
        modelBuilder.Entity<Permission>().HasData(new { Id = IdentityRolesManagePermissionId, Name = "Identity.Roles.Manage", Description = "Manage platform roles and permissions." });

        modelBuilder.Entity("RolePermissions").HasData(new object[]
        {
            new { RoleId = CustomerRoleId, PermissionId = IdentityProfileReadPermissionId },
            new { RoleId = CustomerRoleId, PermissionId = IdentityTokenRefreshPermissionId },
            new { RoleId = CustomerRoleId, PermissionId = KycSubmitPermissionId },
            new { RoleId = CustomerRoleId, PermissionId = CustomerReadPermissionId },
            new { RoleId = CustomerRoleId, PermissionId = CustomerWritePermissionId },
            new { RoleId = CustomerRoleId, PermissionId = PolicyReadPermissionId },
            new { RoleId = CustomerRoleId, PermissionId = PolicyWritePermissionId },
            new { RoleId = KycReviewerRoleId, PermissionId = KycVerifyPermissionId },
            new { RoleId = PolicyUnderwriterRoleId, PermissionId = PolicyReadPermissionId },
            new { RoleId = PolicyUnderwriterRoleId, PermissionId = PolicyWriteAnyPermissionId },
            new { RoleId = PolicyUnderwriterRoleId, PermissionId = PolicyApprovePermissionId },
            new { RoleId = ClaimsAdjusterRoleId, PermissionId = ClaimReadPermissionId },
            new { RoleId = ClaimsAdjusterRoleId, PermissionId = ClaimDecidePermissionId },
            new { RoleId = PaymentOperationsRoleId, PermissionId = PaymentReadPermissionId },
            new { RoleId = PaymentOperationsRoleId, PermissionId = PaymentRefundPermissionId }
        });

        modelBuilder.Entity("RolePermissions").HasData(new object[]
        {
            new { RoleId = SupportAgentRoleId, PermissionId = CustomerReadPermissionId },
            new { RoleId = SupportAgentRoleId, PermissionId = CustomerWritePermissionId },
            new { RoleId = SupportAgentRoleId, PermissionId = PolicyReadPermissionId },
            new { RoleId = SupportAgentRoleId, PermissionId = ClaimReadPermissionId },
            new { RoleId = ComplianceOfficerRoleId, PermissionId = KycAuditReadPermissionId },
            new { RoleId = ComplianceOfficerRoleId, PermissionId = ReportingViewPermissionId },
            new { RoleId = PlatformAdminRoleId, PermissionId = IdentityUsersManagePermissionId },
            new { RoleId = PlatformAdminRoleId, PermissionId = IdentityRolesManagePermissionId },
            new { RoleId = PlatformAdminRoleId, PermissionId = PolicyWriteAnyPermissionId }
        });
    }
}