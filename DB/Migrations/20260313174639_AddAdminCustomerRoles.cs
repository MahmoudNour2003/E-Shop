using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminCustomerRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5eb51801-1b8a-4f2f-a3bf-79e7ea29cd0e", "714a7e22-8a09-453e-ba62-fbe14b6c675e", "Admin", "ADMIN" },
                    { "74945d78-088a-4e05-ba0e-8113a602453d", "a9ce6d5a-ce20-4abf-adbc-22a0238ee2a6", "Customer", "CUSTOMER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5eb51801-1b8a-4f2f-a3bf-79e7ea29cd0e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "74945d78-088a-4e05-ba0e-8113a602453d");
        }
    }
}
