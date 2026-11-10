using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class EtkinlikTurleriGenisletildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OzelTur",
                table: "Etkinlikler",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OzelTur",
                table: "Etkinlikler");
        }
    }
}
