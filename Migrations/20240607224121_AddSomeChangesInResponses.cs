using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hh.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeChangesInResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowed",
                table: "Responses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_VacationId",
                table: "Chats",
                column: "VacationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Vacations_VacationId",
                table: "Chats",
                column: "VacationId",
                principalTable: "Vacations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Vacations_VacationId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_VacationId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "IsAllowed",
                table: "Responses");

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
