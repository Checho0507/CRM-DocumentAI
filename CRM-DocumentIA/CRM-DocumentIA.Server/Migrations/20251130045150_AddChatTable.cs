using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_DocumentIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddChatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChatId",
                table: "insightsHisto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Descripcion",
                value: "Usuario estándar");

            migrationBuilder.CreateIndex(
                name: "IX_insightsHisto_ChatId",
                table: "insightsHisto",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_insightsHisto_Chats_ChatId",
                table: "insightsHisto",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_insightsHisto_Chats_ChatId",
                table: "insightsHisto");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_insightsHisto_ChatId",
                table: "insightsHisto");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "insightsHisto");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Descripcion",
                value: "Usuario est�ndar");
        }
    }
}
