using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKycPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d003"), "Submit identity documents for KYC verification.", "Kyc.Submit" },
                    { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d004"), "Review and decide KYC verification cases.", "Kyc.Verify" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d003"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d004"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d003"), new Guid("6a34f111-5c2b-4a86-95a9-6d622c2fa001") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d43137c8-2faf-4b99-b36a-b71d39a9d003"));
        }
    }
}
