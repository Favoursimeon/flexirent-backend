using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexiRent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "RentalPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RentalPayments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "RentalPayments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "RentalPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminApproved",
                table: "RentalPayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaseId",
                table: "RentalPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "RentalPayments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaystackReference",
                table: "RentalPayments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaystackTransactionId",
                table: "RentalPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "RentalPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RentalPayments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RentalPayments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RentalPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RentalLeases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "RentalLeases",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "RentalLeases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyAmount",
                table: "RentalLeases",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDayOfMonth",
                table: "RentalLeases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyId",
                table: "RentalLeases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "RentalLeases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RentalLeases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RentalLeases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RentalLeases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MobileProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentSchedules_RentalLeases_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "RentalLeases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_LeaseId",
                table: "RentalPayments",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_PaystackReference",
                table: "RentalPayments",
                column: "PaystackReference",
                unique: true,
                filter: "\"PaystackReference\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_ScheduleId",
                table: "RentalPayments",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_UserId",
                table: "RentalPayments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalLeases_PropertyId",
                table: "RentalLeases",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalLeases_Status",
                table: "RentalLeases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentalLeases_TenantId",
                table: "RentalLeases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAccounts_UserId",
                table: "PaymentAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_DueDate",
                table: "PaymentSchedules",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_LeaseId",
                table: "PaymentSchedules",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_Status",
                table: "PaymentSchedules",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalLeases_Properties_PropertyId",
                table: "RentalLeases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalLeases_Users_TenantId",
                table: "RentalLeases",
                column: "TenantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalPayments_PaymentSchedules_ScheduleId",
                table: "RentalPayments",
                column: "ScheduleId",
                principalTable: "PaymentSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalPayments_RentalLeases_LeaseId",
                table: "RentalPayments",
                column: "LeaseId",
                principalTable: "RentalLeases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalPayments_Users_UserId",
                table: "RentalPayments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalLeases_Properties_PropertyId",
                table: "RentalLeases");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalLeases_Users_TenantId",
                table: "RentalLeases");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalPayments_PaymentSchedules_ScheduleId",
                table: "RentalPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalPayments_RentalLeases_LeaseId",
                table: "RentalPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalPayments_Users_UserId",
                table: "RentalPayments");

            migrationBuilder.DropTable(
                name: "PaymentAccounts");

            migrationBuilder.DropTable(
                name: "PaymentSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RentalPayments_LeaseId",
                table: "RentalPayments");

            migrationBuilder.DropIndex(
                name: "IX_RentalPayments_PaystackReference",
                table: "RentalPayments");

            migrationBuilder.DropIndex(
                name: "IX_RentalPayments_ScheduleId",
                table: "RentalPayments");

            migrationBuilder.DropIndex(
                name: "IX_RentalPayments_UserId",
                table: "RentalPayments");

            migrationBuilder.DropIndex(
                name: "IX_RentalLeases_PropertyId",
                table: "RentalLeases");

            migrationBuilder.DropIndex(
                name: "IX_RentalLeases_Status",
                table: "RentalLeases");

            migrationBuilder.DropIndex(
                name: "IX_RentalLeases_TenantId",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "IsAdminApproved",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "LeaseId",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "PaystackReference",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "PaystackTransactionId",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "MonthlyAmount",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "PaymentDayOfMonth",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RentalLeases");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RentalLeases");
        }
    }
}
