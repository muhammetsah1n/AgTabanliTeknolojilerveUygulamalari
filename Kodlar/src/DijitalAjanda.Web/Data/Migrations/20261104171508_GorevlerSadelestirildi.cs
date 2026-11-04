using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DijitalAjanda.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class GorevlerSadelestirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etkinlikler_Kategoriler_KategoriId",
                table: "Etkinlikler");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevler_Kategoriler_KategoriId",
                table: "Gorevler");

            migrationBuilder.DropTable(
                name: "AltGorevler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevler_KategoriId",
                table: "Gorevler");

            migrationBuilder.DropIndex(
                name: "IX_Etkinlikler_KategoriId",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Gorevler");

            migrationBuilder.AddColumn<bool>(
                name: "TamamlandiMi",
                table: "Gorevler",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Bitti (2) durumundaki yapılacaklar tamamlanmış olarak kalır
            migrationBuilder.Sql("UPDATE \"Gorevler\" SET \"TamamlandiMi\" = TRUE WHERE \"Durum\" = 2;");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "KategoriId",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "Oncelik",
                table: "Gorevler");

            migrationBuilder.DropColumn(
                name: "KategoriId",
                table: "Etkinlikler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "Gorevler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"Gorevler\" SET \"Durum\" = 2 WHERE \"TamamlandiMi\";");

            migrationBuilder.DropColumn(
                name: "TamamlandiMi",
                table: "Gorevler");

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Gorevler",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KategoriId",
                table: "Gorevler",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Oncelik",
                table: "Gorevler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KategoriId",
                table: "Etkinlikler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AltGorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GorevId = table.Column<int>(type: "integer", nullable: false),
                    Baslik = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TamamlandiMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AltGorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AltGorevler_Gorevler_GorevId",
                        column: x => x.GorevId,
                        principalTable: "Gorevler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KullaniciId = table.Column<string>(type: "text", nullable: false),
                    Ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Renk = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevler_KategoriId",
                table: "Gorevler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Etkinlikler_KategoriId",
                table: "Etkinlikler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_AltGorevler_GorevId",
                table: "AltGorevler",
                column: "GorevId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_KullaniciId_Ad",
                table: "Kategoriler",
                columns: new[] { "KullaniciId", "Ad" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Etkinlikler_Kategoriler_KategoriId",
                table: "Etkinlikler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevler_Kategoriler_KategoriId",
                table: "Gorevler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
