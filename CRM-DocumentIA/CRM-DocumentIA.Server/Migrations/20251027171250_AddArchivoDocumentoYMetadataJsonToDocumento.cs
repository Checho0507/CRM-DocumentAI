using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_DocumentIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivoDocumentoYMetadataJsonToDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ArchivoDocumento",
                table: "Documentos",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivoMetadataJson",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivoDocumento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ArchivoMetadataJson",
                table: "Documentos");
        }
    }
}
