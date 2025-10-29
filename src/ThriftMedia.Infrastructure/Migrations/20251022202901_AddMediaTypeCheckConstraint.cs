using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThriftMedia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaTypeCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Media_Type",
                schema: "media",
                table: "Media",
                sql: "\"Type\" IN ('book', 'video', 'cdrom', 'vinyl-record', 'eight-track', 'cassette', 'dvd', 'blu-ray', 'magazine', 'comic', 'other', 'unknown')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Media_Type",
                schema: "media",
                table: "Media");
        }
    }
}
