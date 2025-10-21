using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThriftMedia.Data.Migrations
{
    public partial class RenameStoreOwnerToStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaItems_StoreOwner",
                schema: "media",
                table: "MediaItems");

            migrationBuilder.DropTable(
                name: "StoreOwner",
                schema: "media");

            migrationBuilder.RenameColumn(
                name: "StoreOwnerId",
                schema: "media",
                table: "MediaItems",
                newName: "StoreId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaItems_StoreOwnerId",
                schema: "media",
                table: "MediaItems",
                newName: "IX_MediaItems_StoreId");

            migrationBuilder.CreateTable(
                name: "Store",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Store__3214EC07271D525F", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MediaItems_Store",
                schema: "media",
                table: "MediaItems",
                column: "StoreId",
                principalSchema: "media",
                principalTable: "Store",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaItems_Store",
                schema: "media",
                table: "MediaItems");

            migrationBuilder.DropTable(
                name: "Store",
                schema: "media");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                schema: "media",
                table: "MediaItems",
                newName: "StoreOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaItems_StoreId",
                schema: "media",
                table: "MediaItems",
                newName: "IX_MediaItems_StoreOwnerId");

            migrationBuilder.CreateTable(
                name: "StoreOwner",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StoreName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StoreOwn__3214EC07271D525F", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MediaItems_StoreOwner",
                schema: "media",
                table: "MediaItems",
                column: "StoreOwnerId",
                principalSchema: "media",
                principalTable: "StoreOwner",
                principalColumn: "Id");
        }
    }
}