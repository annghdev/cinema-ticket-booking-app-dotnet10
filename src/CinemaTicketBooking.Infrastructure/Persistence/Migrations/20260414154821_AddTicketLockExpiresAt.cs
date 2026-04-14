using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketLockExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockExpiresAt",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Status_LockExpiresAt",
                table: "tickets",
                columns: new[] { "Status", "LockExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_Status_LockExpiresAt",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "LockExpiresAt",
                table: "tickets");
        }
    }
}
