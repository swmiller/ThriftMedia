using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThriftMedia.Data.Migrations
{
    public partial class AddAddressOwnedTypeToStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Address", schema: "media", table: "Store");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                schema: "media",
                table: "Store",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                schema: "media",
                table: "Store",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                schema: "media",
                table: "Store",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                schema: "media",
                table: "Store",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                schema: "media",
                table: "Store",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Address_City", schema: "media", table: "Store");
            migrationBuilder.DropColumn(name: "Address_Country", schema: "media", table: "Store");
            migrationBuilder.DropColumn(name: "Address_State", schema: "media", table: "Store");
            migrationBuilder.DropColumn(name: "Address_Street", schema: "media", table: "Store");
            migrationBuilder.DropColumn(name: "Address_ZipCode", schema: "media", table: "Store");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "media",
                table: "Store",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}