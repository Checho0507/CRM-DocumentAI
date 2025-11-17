using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_DocumentIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDocumentosYProcesamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Clientes_ClienteId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Insights_Clientes_ClienteId",
                table: "Insights");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcesosIA_Documentos_DocumentoId",
                table: "ProcesosIA");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TipoProceso",
                table: "ProcesosIA");

            migrationBuilder.DropColumn(
                name: "GeneradoEn",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Insights");

            migrationBuilder.RenameColumn(
                name: "Resultado",
                table: "ProcesosIA",
                newName: "UrlServicio");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "usuario",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "usuario");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuarios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<bool>(
                name: "DobleFactorActivado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaInicio",
                table: "ProcesosIA",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ProcesosIA",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "pendiente",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "ProcesosIA",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoJson",
                table: "ProcesosIA",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TiempoProcesamientoSegundos",
                table: "ProcesosIA",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoProcesamiento",
                table: "ProcesosIA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "analisis_documento");

            migrationBuilder.AddColumn<double>(
                name: "Confianza",
                table: "Insights",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaGeneracion",
                table: "Insights",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "ProcesoIAId",
                table: "Insights",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoInsight",
                table: "Insights",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "general");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Documentos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddColumn<string>(
                name: "ErrorProcesamiento",
                table: "Documentos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoProcesamiento",
                table: "Documentos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProcesamiento",
                table: "Documentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroImagenes",
                table: "Documentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumenDocumento",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TamañoArchivo",
                table: "Documentos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlServicioIA",
                table: "Documentos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Documentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosIA_Estado",
                table: "ProcesosIA",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosIA_FechaInicio",
                table: "ProcesosIA",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Insights_FechaGeneracion",
                table: "Insights",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_Insights_ProcesoIAId",
                table: "Insights",
                column: "ProcesoIAId");

            migrationBuilder.CreateIndex(
                name: "IX_Insights_TipoInsight",
                table: "Insights",
                column: "TipoInsight");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_UsuarioId",
                table: "Documentos",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Clientes_ClienteId",
                table: "Documentos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Usuarios_UsuarioId",
                table: "Documentos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Insights_Clientes_ClienteId",
                table: "Insights",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Insights_ProcesosIA_ProcesoIAId",
                table: "Insights",
                column: "ProcesoIAId",
                principalTable: "ProcesosIA",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesosIA_Documentos_DocumentoId",
                table: "ProcesosIA",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Clientes_ClienteId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Usuarios_UsuarioId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Insights_Clientes_ClienteId",
                table: "Insights");

            migrationBuilder.DropForeignKey(
                name: "FK_Insights_ProcesosIA_ProcesoIAId",
                table: "Insights");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcesosIA_Documentos_DocumentoId",
                table: "ProcesosIA");

            migrationBuilder.DropIndex(
                name: "IX_ProcesosIA_Estado",
                table: "ProcesosIA");

            migrationBuilder.DropIndex(
                name: "IX_ProcesosIA_FechaInicio",
                table: "ProcesosIA");

            migrationBuilder.DropIndex(
                name: "IX_Insights_FechaGeneracion",
                table: "Insights");

            migrationBuilder.DropIndex(
                name: "IX_Insights_ProcesoIAId",
                table: "Insights");

            migrationBuilder.DropIndex(
                name: "IX_Insights_TipoInsight",
                table: "Insights");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_UsuarioId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "ProcesosIA");

            migrationBuilder.DropColumn(
                name: "ResultadoJson",
                table: "ProcesosIA");

            migrationBuilder.DropColumn(
                name: "TiempoProcesamientoSegundos",
                table: "ProcesosIA");

            migrationBuilder.DropColumn(
                name: "TipoProcesamiento",
                table: "ProcesosIA");

            migrationBuilder.DropColumn(
                name: "Confianza",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "FechaGeneracion",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "ProcesoIAId",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "TipoInsight",
                table: "Insights");

            migrationBuilder.DropColumn(
                name: "ArchivoDocumento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ArchivoMetadataJson",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ErrorProcesamiento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "EstadoProcesamiento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FechaProcesamiento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "NumeroImagenes",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ResumenDocumento",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TamañoArchivo",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "UrlServicioIA",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Documentos");

            migrationBuilder.RenameColumn(
                name: "UrlServicio",
                table: "ProcesosIA",
                newName: "Resultado");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "usuario",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "usuario");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuarios",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "DobleFactorActivado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaInicio",
                table: "ProcesosIA",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ProcesosIA",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "pendiente");

            migrationBuilder.AddColumn<string>(
                name: "TipoProceso",
                table: "ProcesosIA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneradoEn",
                table: "Insights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Insights",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Documentos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Clientes_ClienteId",
                table: "Documentos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Insights_Clientes_ClienteId",
                table: "Insights",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesosIA_Documentos_DocumentoId",
                table: "ProcesosIA",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
