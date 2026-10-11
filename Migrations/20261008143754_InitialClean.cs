using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Agences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ville = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstSiege = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TypesCompte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Libelle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TauxInteret = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    SoldeMinimum = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    AutoriseDecouvert = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesCompte", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroClient = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NumeroCNI = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adresse = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenceId = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Actif")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parametres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valeur = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenceId = table.Column<int>(type: "int", nullable: true),
                    AgenceId1 = table.Column<int>(type: "int", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parametres_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parametres_Agences_AgenceId1",
                        column: x => x.AgenceId1,
                        principalTable: "Agences",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomComplet = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotDePasseHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenceId = table.Column<int>(type: "int", nullable: false),
                    DeuxFA_Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeuxFA_Secret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DerniereConnexion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateur_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolesClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comptes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroCompte = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    TypeCompteId = table.Column<int>(type: "int", nullable: false),
                    AgenceId = table.Column<int>(type: "int", nullable: false),
                    Solde = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Actif")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOuverture = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateFermeture = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comptes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comptes_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comptes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comptes_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comptes_TypesCompte_TypeCompteId",
                        column: x => x.TypeCompteId,
                        principalTable: "TypesCompte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomComplet = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenceId = table.Column<int>(type: "int", nullable: true),
                    DeuxFA_Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeuxFA_Secret = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DerniereConnexion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroCarte = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompteId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    TypeCarte = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CompteId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cartes_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cartes_Comptes_CompteId1",
                        column: x => x.CompteId1,
                        principalTable: "Comptes",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Virements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompteSourceId = table.Column<int>(type: "int", nullable: false),
                    CompteDestinationId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Motif = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ControleAML = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DateVirement = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Virements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Virements_Comptes_CompteDestinationId",
                        column: x => x.CompteDestinationId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Virements_Comptes_CompteSourceId",
                        column: x => x.CompteSourceId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Virements_Utilisateur_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateEvenement = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UtilisateurId = table.Column<int>(type: "int", nullable: true),
                    UtilisateurId1 = table.Column<int>(type: "int", nullable: true),
                    AgenceId = table.Column<int>(type: "int", nullable: true),
                    AgenceId1 = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TableConcernee = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnregistrementId = table.Column<int>(type: "int", nullable: true),
                    ValeurAvant = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValeurApres = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdresseIP = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLog_Agences_AgenceId1",
                        column: x => x.AgenceId1,
                        principalTable: "Agences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLog_Utilisateur_UtilisateurId1",
                        column: x => x.UtilisateurId1,
                        principalTable: "Utilisateur",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLog_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Prets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CompteDebitId = table.Column<int>(type: "int", nullable: false),
                    CompteDebitId1 = table.Column<int>(type: "int", nullable: false),
                    ValideParId = table.Column<int>(type: "int", nullable: true),
                    ValideParId1 = table.Column<int>(type: "int", nullable: true),
                    MontantPrincipal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TauxInteret = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DureeMois = table.Column<int>(type: "int", nullable: false),
                    Mensualite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDemande = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prets_Comptes_CompteDebitId",
                        column: x => x.CompteDebitId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prets_Comptes_CompteDebitId1",
                        column: x => x.CompteDebitId1,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prets_Utilisateur_ValideParId1",
                        column: x => x.ValideParId1,
                        principalTable: "Utilisateur",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prets_Utilisateurs_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SessionsUtilisateur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId1 = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdresseIP = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeuxFA_Valide = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DateConnexion = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionsUtilisateur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionsUtilisateur_Utilisateur_UtilisateurId1",
                        column: x => x.UtilisateurId1,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionsUtilisateur_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UtilisateursClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilisateursClaims_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UtilisateursLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UtilisateursLogins_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UtilisateursRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UtilisateursRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UtilisateursRoles_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UtilisateursTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UtilisateursTokens_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VerificationsKYC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    TypeDocument = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroDocument = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheminDocument = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VerifieParId = table.Column<int>(type: "int", nullable: true),
                    VerifieParId1 = table.Column<int>(type: "int", nullable: true),
                    DateVerification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationsKYC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Utilisateur_VerifieParId1",
                        column: x => x.VerifieParId1,
                        principalTable: "Utilisateur",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Utilisateurs_VerifieParId",
                        column: x => x.VerifieParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EcheancesPret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PretId = table.Column<int>(type: "int", nullable: false),
                    NumeroEcheance = table.Column<int>(type: "int", nullable: false),
                    Capital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Interet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcheancesPret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcheancesPret_Prets_PretId",
                        column: x => x.PretId,
                        principalTable: "Prets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeOperation = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompteId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    AgenceId = table.Column<int>(type: "int", nullable: false),
                    VirementId = table.Column<int>(type: "int", nullable: true),
                    PretId = table.Column<int>(type: "int", nullable: true),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOperation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Commentaire = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Prets_PretId",
                        column: x => x.PretId,
                        principalTable: "Prets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Operations_Utilisateur_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Virements_VirementId",
                        column: x => x.VirementId,
                        principalTable: "Virements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlertesAML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: true),
                    TypeAlerte = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Niveau = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Ouverte")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TraiteParId = table.Column<int>(type: "int", nullable: true),
                    DateAlerte = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    DateTraitement = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertesAML", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Utilisateur_TraiteParId",
                        column: x => x.TraiteParId,
                        principalTable: "Utilisateur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ecritures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    CompteId = table.Column<int>(type: "int", nullable: false),
                    Sens = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "BIF")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEcriture = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    Libelle = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecritures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ecritures_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ecritures_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Agences_Code",
                table: "Agences",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_ClientId",
                table: "AlertesAML",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_OperationId",
                table: "AlertesAML",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_Statut",
                table: "AlertesAML",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_TraiteParId",
                table: "AlertesAML",
                column: "TraiteParId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_AgenceId",
                table: "AuditLog",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_AgenceId1",
                table: "AuditLog",
                column: "AgenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_DateEvenement",
                table: "AuditLog",
                column: "DateEvenement");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UtilisateurId",
                table: "AuditLog",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UtilisateurId1",
                table: "AuditLog",
                column: "UtilisateurId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_ClientId",
                table: "Cartes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_CompteId",
                table: "Cartes",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_CompteId1",
                table: "Cartes",
                column: "CompteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_NumeroCarte",
                table: "Cartes",
                column: "NumeroCarte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AgenceId",
                table: "Clients",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NumeroClient",
                table: "Clients",
                column: "NumeroClient",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NumeroCNI",
                table: "Clients",
                column: "NumeroCNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_AgenceId",
                table: "Comptes",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_ClientId",
                table: "Comptes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_ClientId1",
                table: "Comptes",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_NumeroCompte",
                table: "Comptes",
                column: "NumeroCompte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_TypeCompteId",
                table: "Comptes",
                column: "TypeCompteId");

            migrationBuilder.CreateIndex(
                name: "IX_EcheancesPret_DateEcheance",
                table: "EcheancesPret",
                column: "DateEcheance");

            migrationBuilder.CreateIndex(
                name: "IX_EcheancesPret_PretId_NumeroEcheance",
                table: "EcheancesPret",
                columns: new[] { "PretId", "NumeroEcheance" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_CompteId",
                table: "Ecritures",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_OperationId",
                table: "Ecritures",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AgenceId",
                table: "Operations",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CompteId",
                table: "Operations",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_DateOperation",
                table: "Operations",
                column: "DateOperation");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_PretId",
                table: "Operations",
                column: "PretId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Reference",
                table: "Operations",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Statut",
                table: "Operations",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UtilisateurId",
                table: "Operations",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_VirementId",
                table: "Operations",
                column: "VirementId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_AgenceId",
                table: "Parametres",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_AgenceId1",
                table: "Parametres",
                column: "AgenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_Cle_AgenceId",
                table: "Parametres",
                columns: new[] { "Cle", "AgenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ClientId",
                table: "Prets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_CompteDebitId",
                table: "Prets",
                column: "CompteDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_CompteDebitId1",
                table: "Prets",
                column: "CompteDebitId1");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_Reference",
                table: "Prets",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prets_Statut",
                table: "Prets",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ValideParId",
                table: "Prets",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ValideParId1",
                table: "Prets",
                column: "ValideParId1");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_RoleId",
                table: "RolesClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_Token",
                table: "SessionsUtilisateur",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_UtilisateurId",
                table: "SessionsUtilisateur",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_UtilisateurId1",
                table: "SessionsUtilisateur",
                column: "UtilisateurId1");

            migrationBuilder.CreateIndex(
                name: "IX_TypesCompte_Code",
                table: "TypesCompte",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateur_AgenceId",
                table: "Utilisateur",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Utilisateurs",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_AgenceId",
                table: "Utilisateurs",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_ClientId",
                table: "Utilisateurs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Role",
                table: "Utilisateurs",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Utilisateurs",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursClaims_UserId",
                table: "UtilisateursClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursLogins_UserId",
                table: "UtilisateursLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursRoles_RoleId",
                table: "UtilisateursRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_ClientId",
                table: "VerificationsKYC",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_ClientId1",
                table: "VerificationsKYC",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_VerifieParId",
                table: "VerificationsKYC",
                column: "VerifieParId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_VerifieParId1",
                table: "VerificationsKYC",
                column: "VerifieParId1");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteDestinationId",
                table: "Virements",
                column: "CompteDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteSourceId",
                table: "Virements",
                column: "CompteSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_Reference",
                table: "Virements",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Virements_Statut",
                table: "Virements",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_UtilisateurId",
                table: "Virements",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertesAML");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "Cartes");

            migrationBuilder.DropTable(
                name: "EcheancesPret");

            migrationBuilder.DropTable(
                name: "Ecritures");

            migrationBuilder.DropTable(
                name: "Parametres");

            migrationBuilder.DropTable(
                name: "RolesClaims");

            migrationBuilder.DropTable(
                name: "SessionsUtilisateur");

            migrationBuilder.DropTable(
                name: "UtilisateursClaims");

            migrationBuilder.DropTable(
                name: "UtilisateursLogins");

            migrationBuilder.DropTable(
                name: "UtilisateursRoles");

            migrationBuilder.DropTable(
                name: "UtilisateursTokens");

            migrationBuilder.DropTable(
                name: "VerificationsKYC");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Prets");

            migrationBuilder.DropTable(
                name: "Virements");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Comptes");

            migrationBuilder.DropTable(
                name: "Utilisateur");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "TypesCompte");

            migrationBuilder.DropTable(
                name: "Agences");
        }
    }
}
