using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFraudFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "KycRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FraudFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FlagType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsResolved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudFlags_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_KycRecords_UserId1",
                table: "KycRecords",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FraudFlags_UserId",
                table: "FraudFlags",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_KycRecords_Users_UserId1",
                table: "KycRecords",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KycRecords_Users_UserId1",
                table: "KycRecords");

            migrationBuilder.DropTable(
                name: "FraudFlags");

            migrationBuilder.DropIndex(
                name: "IX_KycRecords_UserId1",
                table: "KycRecords");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "KycRecords");
        }
    }
}
