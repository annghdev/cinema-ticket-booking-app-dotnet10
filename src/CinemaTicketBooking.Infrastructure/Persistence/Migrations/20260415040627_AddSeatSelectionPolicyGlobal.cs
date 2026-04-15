using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatSelectionPolicyGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var policyId = new Guid("7BFF00FA-5F87-4A7A-A598-039F6B3060F2");
            var createdAt = new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.Zero);

            migrationBuilder.CreateTable(
                name: "seat_selection_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsGlobalDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTicketsPerCheckout = table.Column<int>(type: "integer", nullable: false),
                    MaxRowsPerCheckout = table.Column<int>(type: "integer", nullable: false),
                    OrphanSeatLevel = table.Column<int>(type: "integer", nullable: false),
                    CheckerboardLevel = table.Column<int>(type: "integer", nullable: false),
                    SplitAcrossAisleLevel = table.Column<int>(type: "integer", nullable: false),
                    IsolatedRowEndSingleLevel = table.Column<int>(type: "integer", nullable: false),
                    MisalignedRowsLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seat_selection_policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_seat_selection_policies_IsGlobalDefault_IsActive",
                table: "seat_selection_policies",
                columns: new[] { "IsGlobalDefault", "IsActive" });

            migrationBuilder.InsertData(
                table: "seat_selection_policies",
                columns: new[]
                {
                    "Id",
                    "Name",
                    "IsGlobalDefault",
                    "IsActive",
                    "MaxTicketsPerCheckout",
                    "MaxRowsPerCheckout",
                    "OrphanSeatLevel",
                    "CheckerboardLevel",
                    "SplitAcrossAisleLevel",
                    "IsolatedRowEndSingleLevel",
                    "MisalignedRowsLevel",
                    "CreatedAt",
                    "CreatedBy"
                },
                values: new object[]
                {
                    policyId,
                    "Global default pre-checkout seat policy",
                    true,
                    true,
                    8,
                    2,
                    2,
                    2,
                    2,
                    1,
                    2,
                    createdAt,
                    "system"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "seat_selection_policies",
                keyColumn: "Id",
                keyValue: new Guid("7BFF00FA-5F87-4A7A-A598-039F6B3060F2"));

            migrationBuilder.DropTable(
                name: "seat_selection_policies");
        }
    }
}
