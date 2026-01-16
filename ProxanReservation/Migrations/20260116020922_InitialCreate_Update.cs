using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProxanReservation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "reservations");

            migrationBuilder.RenameTable(
                name: "Events",
                newName: "events");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "reservations",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reservations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "reservations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "reservations",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "reservations",
                newName: "event_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "reservations",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_EventId",
                table: "reservations",
                newName: "IX_reservations_event_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "events",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "events",
                newName: "capacity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "events",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "AvailableCapacity",
                table: "events",
                newName: "available_capacity");

            migrationBuilder.AlterColumn<string>(
                name: "state",
                table: "reservations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "events",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_reservations",
                table: "reservations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_events",
                table: "events",
                column: "id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_events_event_id",
                table: "reservations",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservations_events_event_id",
                table: "reservations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reservations",
                table: "reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_events",
                table: "events");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "events");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "events");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "events");

            migrationBuilder.RenameTable(
                name: "reservations",
                newName: "Reservations");

            migrationBuilder.RenameTable(
                name: "events",
                newName: "Events");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "Reservations",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Reservations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Reservations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "Reservations",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "Reservations",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Reservations",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_reservations_event_id",
                table: "Reservations",
                newName: "IX_Reservations_EventId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Events",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "capacity",
                table: "Events",
                newName: "Capacity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Events",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "available_capacity",
                table: "Events",
                newName: "AvailableCapacity");

            migrationBuilder.AlterColumn<int>(
                name: "State",
                table: "Reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
