using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPaymentExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TicketStatus enum changed from (Available=0, Locking=1, Sold=2)
            // to (Available=0, Locking=1, PendingPayment=2, Sold=3).
            // Remap existing Sold rows to preserve semantics after enum expansion.
            migrationBuilder.Sql("""
                UPDATE tickets
                SET "Status" = 3
                WHERE "Status" = 2;
                """);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentExpiresAt",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Status_PaymentExpiresAt",
                table: "tickets",
                columns: new[] { "Status", "PaymentExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE tickets
                SET "Status" = 2
                WHERE "Status" = 3;
                """);

            migrationBuilder.DropIndex(
                name: "IX_tickets_Status_PaymentExpiresAt",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "PaymentExpiresAt",
                table: "tickets");
        }
    }
}
