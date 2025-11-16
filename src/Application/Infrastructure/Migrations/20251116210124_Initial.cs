using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tech_challange");

            migrationBuilder.CreateTable(
                name: "motorcycle",
                schema: "tech_challange",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", maxLength: 4, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    license_plate = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motorcycle", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycle_notification",
                schema: "tech_challange",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    license_plate = table.Column<string>(type: "text", nullable: false),
                    notification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motorcycle_notification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rental_plan",
                schema: "tech_challange",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    daily_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    penalty_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental_plan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rider",
                schema: "tech_challange",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    cnh = table.Column<string>(type: "text", nullable: false),
                    cnh_type = table.Column<string>(type: "text", nullable: false),
                    cnh_image_url = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rider", x => x.id);
                    table.CheckConstraint("CK_Rider_CnhType", "cnh_type IN ('A', 'B', 'A+B')");
                });

            migrationBuilder.CreateTable(
                name: "rental",
                schema: "tech_challange",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rental_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    return_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expected_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    penalty_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental", x => x.id);
                    table.ForeignKey(
                        name: "fk_rental_motorcycle_motorcycle_id",
                        column: x => x.motorcycle_id,
                        principalSchema: "tech_challange",
                        principalTable: "motorcycle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rental_rental_plan_rental_plan_id",
                        column: x => x.rental_plan_id,
                        principalSchema: "tech_challange",
                        principalTable: "rental_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rental_rider_rider_id",
                        column: x => x.rider_id,
                        principalSchema: "tech_challange",
                        principalTable: "rider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "tech_challange",
                table: "rental_plan",
                columns: new[] { "id", "created_at", "daily_amount", "days", "penalty_percentage", "updated_at" },
                values: new object[,]
                {
                    { new Guid("0199d9bc-f73b-70bf-9e79-473208dc909e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 18m, 50, 0m, null },
                    { new Guid("0199d9bc-f73b-72de-8e2f-d18545ceca6f"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 28m, 15, 40m, null },
                    { new Guid("0199d9bc-f73b-74a7-9843-b6c79c2d40c6"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30m, 7, 20m, null },
                    { new Guid("0199d9bc-f73b-7d14-895c-bb90b96fe378"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20m, 45, 0m, null },
                    { new Guid("0199d9bc-f73b-7e14-b426-61fc9704d44d"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22m, 30, 0m, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_motorcycle_identifier",
                schema: "tech_challange",
                table: "motorcycle",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_motorcycle_license_plate",
                schema: "tech_challange",
                table: "motorcycle",
                column: "license_plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rental_motorcycle_id",
                schema: "tech_challange",
                table: "rental",
                column: "motorcycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_rental_rental_plan_id",
                schema: "tech_challange",
                table: "rental",
                column: "rental_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_rental_rider_id",
                schema: "tech_challange",
                table: "rental",
                column: "rider_id");

            migrationBuilder.CreateIndex(
                name: "ix_rider_cnh",
                schema: "tech_challange",
                table: "rider",
                column: "cnh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rider_cnpj",
                schema: "tech_challange",
                table: "rider",
                column: "cnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "motorcycle_notification",
                schema: "tech_challange");

            migrationBuilder.DropTable(
                name: "rental",
                schema: "tech_challange");

            migrationBuilder.DropTable(
                name: "motorcycle",
                schema: "tech_challange");

            migrationBuilder.DropTable(
                name: "rental_plan",
                schema: "tech_challange");

            migrationBuilder.DropTable(
                name: "rider",
                schema: "tech_challange");
        }
    }
}
