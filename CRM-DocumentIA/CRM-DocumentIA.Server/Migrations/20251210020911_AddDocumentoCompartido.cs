using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_DocumentIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoCompartido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosCompartidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioPropietarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCompartidoId = table.Column<int>(type: "int", nullable: false),
                    FechaCompartido = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Permiso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "lectura"),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosCompartidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosCompartidos_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosCompartidos_Usuarios_UsuarioCompartidoId",
                        column: x => x.UsuarioCompartidoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosCompartidos_Usuarios_UsuarioPropietarioId",
                        column: x => x.UsuarioPropietarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosCompartidos_DocumentoId_UsuarioCompartidoId",
                table: "DocumentosCompartidos",
                columns: new[] { "DocumentoId", "UsuarioCompartidoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosCompartidos_FechaCompartido",
                table: "DocumentosCompartidos",
                column: "FechaCompartido");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosCompartidos_UsuarioCompartidoId",
                table: "DocumentosCompartidos",
                column: "UsuarioCompartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosCompartidos_UsuarioPropietarioId",
                table: "DocumentosCompartidos",
                column: "UsuarioPropietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosCompartidos");
        }
    }
}
