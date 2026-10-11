using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre9ConformiteInterAgences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompensationsAgences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenceId = table.Column<int>(type: "int", nullable: false),
                    CompteCompensationId = table.Column<int>(type: "int", nullable: true),
                    CodeCompteGeneral = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalDebits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Solde = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateCompensation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateReglement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnCours")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompensationsAgences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompensationsAgences_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompensationsAgences_Comptes_CompteCompensationId",
                        column: x => x.CompteCompensationId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeclarationsSoupcon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlerteId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    TypeDeclaration = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Contenu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifSuspicion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Brouillon")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateTransmission = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateAccuseReception = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RedigeParId = table.Column<int>(type: "int", nullable: true),
                    ReponseCNRF = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeclarationsSoupcon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationsSoupcon_AlertesAML_AlerteId",
                        column: x => x.AlerteId,
                        principalTable: "AlertesAML",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationsSoupcon_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationsSoupcon_Utilisateurs_RedigeParId",
                        column: x => x.RedigeParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SeuilsAML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeOperation = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Toutes")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Actif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeuilsAML", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VirementsInterAgences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VirementId = table.Column<int>(type: "int", nullable: false),
                    AgenceSourceId = table.Column<int>(type: "int", nullable: false),
                    AgenceDestinationId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateCompensation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateReglement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MotifRejet = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValideParId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirementsInterAgences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirementsInterAgences_Agences_AgenceDestinationId",
                        column: x => x.AgenceDestinationId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VirementsInterAgences_Agences_AgenceSourceId",
                        column: x => x.AgenceSourceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VirementsInterAgences_Utilisateurs_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VirementsInterAgences_Virements_VirementId",
                        column: x => x.VirementId,
                        principalTable: "Virements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SeuilsAML",
                columns: new[] { "Id", "Actif", "Code", "DateModification", "Devise", "Libelle", "Montant", "TypeOperation" },
                values: new object[,]
                {
                    { 1, true, "VIGILANCE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BIF", "Vigilance renforcée", 5000000m, "Toutes" },
                    { 2, true, "DECLARATION", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BIF", "Déclaration espèces CNRF", 10000000m, "Depot" },
                    { 3, true, "DECLARATION", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BIF", "Déclaration espèces CNRF", 10000000m, "Retrait" },
                    { 4, true, "BLOCAGE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BIF", "Blocage automatique", 50000000m, "Toutes" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompensationsAgences_AgenceId_DateCompensation",
                table: "CompensationsAgences",
                columns: new[] { "AgenceId", "DateCompensation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompensationsAgences_CompteCompensationId",
                table: "CompensationsAgences",
                column: "CompteCompensationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsSoupcon_AlerteId_TypeDeclaration",
                table: "DeclarationsSoupcon",
                columns: new[] { "AlerteId", "TypeDeclaration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsSoupcon_ClientId",
                table: "DeclarationsSoupcon",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsSoupcon_RedigeParId",
                table: "DeclarationsSoupcon",
                column: "RedigeParId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationsSoupcon_Reference",
                table: "DeclarationsSoupcon",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeuilsAML_Code_TypeOperation",
                table: "SeuilsAML",
                columns: new[] { "Code", "TypeOperation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_AgenceDestinationId",
                table: "VirementsInterAgences",
                column: "AgenceDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_AgenceSourceId",
                table: "VirementsInterAgences",
                column: "AgenceSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_Reference",
                table: "VirementsInterAgences",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_Statut",
                table: "VirementsInterAgences",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_ValideParId",
                table: "VirementsInterAgences",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "IX_VirementsInterAgences_VirementId",
                table: "VirementsInterAgences",
                column: "VirementId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompensationsAgences");

            migrationBuilder.DropTable(
                name: "DeclarationsSoupcon");

            migrationBuilder.DropTable(
                name: "SeuilsAML");

            migrationBuilder.DropTable(
                name: "VirementsInterAgences");
        }
    }
}
