using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryLogs_Users_PerformedByUserId",
                table: "InventoryLogs");

            migrationBuilder.RenameColumn(
                name: "PerformedByUserId",
                table: "InventoryLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ChangeQuantity",
                table: "InventoryLogs",
                newName: "QuantityChange");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryLogs_PerformedByUserId",
                table: "InventoryLogs",
                newName: "IX_InventoryLogs_UserId");

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "InventoryLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Inventories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 7, 17, 48, 49, 406, DateTimeKind.Utc).AddTicks(1096));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 7, 17, 48, 49, 406, DateTimeKind.Utc).AddTicks(1099));

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryLogs_Users_UserId",
                table: "InventoryLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryLogs_Users_UserId",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "InventoryLogs",
                newName: "PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "QuantityChange",
                table: "InventoryLogs",
                newName: "ChangeQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryLogs_UserId",
                table: "InventoryLogs",
                newName: "IX_InventoryLogs_PerformedByUserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryLogs_Users_PerformedByUserId",
                table: "InventoryLogs",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
