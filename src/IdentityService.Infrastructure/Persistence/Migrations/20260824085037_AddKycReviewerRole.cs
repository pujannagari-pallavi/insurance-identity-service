using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKycReviewerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa002"), "Reviews and decides KYC verification cases.", "KycReviewer" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d004"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa002") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d004"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa002") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa002"));
        }
    }
}
