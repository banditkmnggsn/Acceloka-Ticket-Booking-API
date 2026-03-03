using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acceloka.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tickets");
        }
    }
}
