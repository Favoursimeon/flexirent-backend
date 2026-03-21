using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexiRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePreferencesAndDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletionReason",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeletionRequested",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionRequestedAt",
                table: "Profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "Profiles",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinBedrooms",
                table: "Profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPrice",
                table: "Profiles",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredRegion",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PropertyAlerts",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionReason",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DeletionRequested",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DeletionRequestedAt",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "MinBedrooms",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "MinPrice",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PreferredRegion",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PropertyAlerts",
                table: "Profiles");
        }
    }
}
