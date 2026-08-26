using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), "Read policy records.", "Policy.Read" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d006"), "Create or update own policy records.", "Policy.Write" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"), "Create or update policy records for any customer.", "Policy.Write.Any" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d008"), "Approve or decline policy applications.", "Policy.Approve" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"), "Read claim records.", "Claim.Read" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d010"), "Approve or decline claim cases.", "Claim.Decide" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d011"), "Read payment records.", "Payment.Read" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d012"), "Process approved payment refunds.", "Payment.Refund" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"), "Read customer records.", "Customer.Read" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"), "Create or update customer records.", "Customer.Write" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d015"), "Read KYC audit activity.", "Kyc.Audit.Read" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d016"), "View operational and compliance reports.", "Reporting.View" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d017"), "Manage platform user access.", "Identity.Users.Manage" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d018"), "Manage platform roles and permissions.", "Identity.Roles.Manage" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003"), "Reviews and approves insurance policy applications.", "PolicyUnderwriter" },
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004"), "Reviews and decides insurance claim cases.", "ClaimsAdjuster" },
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005"), "Handles payment operations and refunds.", "PaymentOperations" },
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006"), "Assists customers with account and policy enquiries.", "SupportAgent" },
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007"), "Audits KYC activity and reviews compliance reporting.", "ComplianceOfficer" },
                    { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008"), "Manages platform users, roles, and operational access.", "PlatformAdmin" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d006"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d008"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d010"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d011"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d012"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d015"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d016"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d017"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d018"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d006"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d008"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d010"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d011"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d012"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d015"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d016"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d017"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d018"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d005"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d006"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d007"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d009"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d010"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d011"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d012"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d013"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d014"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d015"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d016"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d017"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d018"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa003"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa004"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa005"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa006"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa007"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa008"));
        }
    }
}
