using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre12PretsEtCartes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commentaire",
                table: "Prets",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MontantRembourse",
                table: "Prets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotifRejet",
                table: "Prets",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "SoldeRestant",
                table: "Prets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TypePretId",
                table: "Prets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "Penalite",
                table: "EcheancesPret",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldeRestant",
                table: "EcheancesPret",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotifBlocage",
                table: "Cartes",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NumeroCarteMasque",
                table: "Cartes",
                type: "varchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "**** **** **** 0000")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PlafondPaiementEnLigneJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlafondPaiementJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlafondRetraitJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "TransactionsCarte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CarteId = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: true),
                    TypeTransaction = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Refusee")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifRefus = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateTransaction = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionsCarte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionsCarte_Cartes_CarteId",
                        column: x => x.CarteId,
                        principalTable: "Cartes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionsCarte_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TypesPret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontantMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantMax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DureeMinMois = table.Column<int>(type: "int", nullable: false),
                    DureeMaxMois = table.Column<int>(type: "int", nullable: false),
                    TauxMin = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TauxMax = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Actif = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesPret", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "TypesPret",
                columns: new[] { "Id", "Actif", "Code", "DateCreation", "DureeMaxMois", "DureeMinMois", "Libelle", "MontantMax", "MontantMin", "TauxMax", "TauxMin" },
                values: new object[,]
                {
                    { 1, true, "PERSONNEL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 60, 3, "Prêt personnel", 50000000m, 100000m, 18m, 10m },
                    { 2, true, "HABITAT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 240, 12, "Prêt habitat", 500000000m, 1000000m, 15m, 8m },
                    { 3, true, "URGENCE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 24, 1, "Prêt d'urgence", 10000000m, 50000m, 24m, 12m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prets_TypePretId",
                table: "Prets",
                column: "TypePretId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCarte_CarteId_DateTransaction",
                table: "TransactionsCarte",
                columns: new[] { "CarteId", "DateTransaction" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCarte_OperationId",
                table: "TransactionsCarte",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TypesPret_Code",
                table: "TypesPret",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prets_TypesPret_TypePretId",
                table: "Prets",
                column: "TypePretId",
                principalTable: "TypesPret",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prets_TypesPret_TypePretId",
                table: "Prets");

            migrationBuilder.DropTable(
                name: "TransactionsCarte");

            migrationBuilder.DropTable(
                name: "TypesPret");

            migrationBuilder.DropIndex(
                name: "IX_Prets_TypePretId",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "Commentaire",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "MontantRembourse",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "MotifRejet",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "SoldeRestant",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "TypePretId",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "Penalite",
                table: "EcheancesPret");

            migrationBuilder.DropColumn(
                name: "SoldeRestant",
                table: "EcheancesPret");

            migrationBuilder.DropColumn(
                name: "MotifBlocage",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "NumeroCarteMasque",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "PlafondPaiementEnLigneJournalier",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "PlafondPaiementJournalier",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "PlafondRetraitJournalier",
                table: "Cartes");
        }
    }
}
