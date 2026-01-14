using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Orders",
                newName: "Ward");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Orders",
                newName: "City");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 7, 16, 12, 57, 622, DateTimeKind.Utc).AddTicks(2974));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 7, 16, 12, 57, 622, DateTimeKind.Utc).AddTicks(2977));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Ward",
                table: "Orders",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Orders",
                newName: "District");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 3, 7, 22, 31, 307, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 3, 7, 22, 31, 307, DateTimeKind.Utc).AddTicks(6804));
        }
    }
}
