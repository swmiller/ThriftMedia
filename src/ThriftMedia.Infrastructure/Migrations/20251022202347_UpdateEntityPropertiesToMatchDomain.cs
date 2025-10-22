using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThriftMedia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityPropertiesToMatchDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "media",
                table: "Store");

            migrationBuilder.RenameColumn(
                name: "StoreName",
                schema: "media",
                table: "Store",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "MediaType",
                schema: "media",
                table: "Media",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "media",
                table: "Media",
                newName: "ImageUri");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                schema: "media",
                table: "Media",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessLicenseImageUri",
                schema: "media",
                table: "Store",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                schema: "media",
                table: "Media",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OcrPayloadJson",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "media",
                table: "Store",
                newName: "StoreName");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "media",
                table: "Media",
                newName: "MediaType");

            migrationBuilder.RenameColumn(
                name: "ImageUri",
                schema: "media",
                table: "Media",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "media",
                table: "Media",
                newName: "MediaId");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessLicenseImageUri",
                schema: "media",
                table: "Store",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "media",
                table: "Store",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OcrPayloadJson",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
