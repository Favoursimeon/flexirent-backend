using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace FlexiRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Properties",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerMonth",
                table: "Properties",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Properties",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Properties",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "AreaSqft",
                table: "Properties",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "Properties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                table: "Properties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Properties",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Properties",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Properties",
                type: "tsvector",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Properties",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Properties",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PropertyImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyImages_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Region",
                table: "Properties",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_SearchVector",
                table: "Properties",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Status",
                table: "Properties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_PropertyId",
                table: "WishlistItems",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_PropertyId",
                table: "WishlistItems",
                columns: new[] { "UserId", "PropertyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Users_OwnerId",
                table: "Properties",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Users_OwnerId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "PropertyImages");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_Region",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_SearchVector",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_Status",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AreaSqft",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Properties");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Properties",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerMonth",
                table: "Properties",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Properties",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);
        }
    }
}
