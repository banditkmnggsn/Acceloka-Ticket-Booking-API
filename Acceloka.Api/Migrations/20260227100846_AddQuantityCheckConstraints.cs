using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acceloka.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Tickets_Quota_Positive",
                table: "Tickets",
                sql: "\"Quota\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookedTicketDetails_Quantity_Positive",
                table: "BookedTicketDetails",
                sql: "\"Quantity\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tickets_Quota_Positive",
                table: "Tickets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BookedTicketDetails_Quantity_Positive",
                table: "BookedTicketDetails");
        }
    }
}
