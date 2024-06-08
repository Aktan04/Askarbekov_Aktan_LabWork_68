using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hh.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFidldToPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPublished",
                table: "Vacations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Resumes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isPublished",
                table: "Vacations");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Resumes");
        }
    }
}
