using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReleaseDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCurrentToPostAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "PostAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "PostAttachments");
        }
    }
}
