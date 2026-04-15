using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSeatReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_ShowTimeId",
                table: "tickets");

            migrationBuilder.AddColumn<string>(
                name: "SeatCode",
                table: "tickets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeatId",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE tickets
                SET "SeatCode" = COALESCE(
                    NULLIF(REPLACE(split_part("Description", ' - ', 1), 'Seat ', ''), ''),
                    NULLIF(regexp_replace("Code", '^.*-', ''), '')
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE tickets
                SET "SeatCode" = 'UNK-' || left("Id"::text, 8)
                WHERE "SeatCode" IS NULL OR btrim("SeatCode") = '';
                """);

            migrationBuilder.Sql(
                """
                UPDATE tickets t
                SET "SeatId" = s."Id"
                FROM show_times st
                JOIN seats s
                    ON s."ScreenId" = st."ScreenId"
                WHERE st."Id" = t."ShowTimeId"
                  AND s."Code" = t."SeatCode";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SeatCode",
                table: "tickets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ShowTimeId_SeatCode",
                table: "tickets",
                columns: new[] { "ShowTimeId", "SeatCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_ShowTimeId_SeatCode",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "SeatCode",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "tickets");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ShowTimeId",
                table: "tickets",
                column: "ShowTimeId");
        }
    }
}
