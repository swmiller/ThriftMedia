using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ThriftMedia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MissedUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS "pgcrypto";""");
            migrationBuilder.Sql("""ALTER TABLE media."Media" ALTER COLUMN "Id" DROP IDENTITY IF EXISTS;""");
            migrationBuilder.Sql("""ALTER TABLE media."Media" ALTER COLUMN "Id" TYPE uuid USING gen_random_uuid();""");
            migrationBuilder.Sql("""ALTER TABLE media."Media" ALTER COLUMN "Id" SET DEFAULT gen_random_uuid();""");

            //migrationBuilder.AlterColumn<Guid>(
            //    name: "Id",
            //    schema: "media",
            //    table: "Media",
            //    type: "uuid",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "integer")
            //    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Author",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "media",
                table: "Media",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "media",
                table: "Media",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                schema: "media",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "media",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Price",
                schema: "media",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "media",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "media",
                table: "Media");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "media",
                table: "Media",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
