using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadCheck.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupJoinCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJoinCodeEnabled",
                table: "Groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "Groups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinCodeCreatedAt",
                table: "Groups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_JoinCode",
                table: "Groups",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Groups_JoinCode",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsJoinCodeEnabled",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "JoinCodeCreatedAt",
                table: "Groups");
        }
    }
}
