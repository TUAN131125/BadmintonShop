using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ImportPrice",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 21, 22, 57, 605, DateTimeKind.Utc).AddTicks(2344));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 21, 22, 57, 605, DateTimeKind.Utc).AddTicks(2348));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "ProductVariants");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 16, 3, 29, 988, DateTimeKind.Utc).AddTicks(581));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 16, 3, 29, 988, DateTimeKind.Utc).AddTicks(584));
        }
    }
}
