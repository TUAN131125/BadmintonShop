using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeInventoryAndVariantSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "ProductVariants");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductVariants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPerUnit",
                table: "InventoryLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockAfter",
                table: "InventoryLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 17, 32, 16, 97, DateTimeKind.Utc).AddTicks(8867));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 17, 32, 16, 97, DateTimeKind.Utc).AddTicks(8871));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CostPerUnit",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "StockAfter",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Inventories");

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
                value: new DateTime(2026, 1, 9, 21, 25, 4, 638, DateTimeKind.Utc).AddTicks(1232));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 21, 25, 4, 638, DateTimeKind.Utc).AddTicks(1235));
        }
    }
}
