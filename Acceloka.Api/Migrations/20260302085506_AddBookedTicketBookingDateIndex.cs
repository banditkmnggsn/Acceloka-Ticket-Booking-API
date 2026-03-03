using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acceloka.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBookedTicketBookingDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BookedTickets_BookingDate",
                table: "BookedTickets",
                column: "BookingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookedTickets_BookingDate",
                table: "BookedTickets");
        }
    }
}
