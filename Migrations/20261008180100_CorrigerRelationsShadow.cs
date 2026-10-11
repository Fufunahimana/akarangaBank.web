using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class CorrigerRelationsShadow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove redundant shadow foreign keys while retaining the legacy
            // Utilisateur table for any archived user records.
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_Agences_AgenceId1",
                table: "AuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_Utilisateur_UtilisateurId1",
                table: "AuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_Cartes_Comptes_CompteId1",
                table: "Cartes");

            migrationBuilder.DropForeignKey(
                name: "FK_Comptes_Clients_ClientId1",
                table: "Comptes");

            migrationBuilder.DropForeignKey(
                name: "FK_Parametres_Agences_AgenceId1",
                table: "Parametres");

            migrationBuilder.DropForeignKey(
                name: "FK_Prets_Comptes_CompteDebitId1",
                table: "Prets");

            migrationBuilder.DropForeignKey(
                name: "FK_Prets_Utilisateur_ValideParId1",
                table: "Prets");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionsUtilisateur_Utilisateur_UtilisateurId1",
                table: "SessionsUtilisateur");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationsKYC_Clients_ClientId1",
                table: "VerificationsKYC");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationsKYC_Utilisateur_VerifieParId1",
                table: "VerificationsKYC");

            migrationBuilder.DropIndex(
                name: "IX_VerificationsKYC_ClientId1",
                table: "VerificationsKYC");

            migrationBuilder.DropIndex(
                name: "IX_VerificationsKYC_VerifieParId1",
                table: "VerificationsKYC");

            migrationBuilder.DropIndex(
                name: "IX_SessionsUtilisateur_UtilisateurId1",
                table: "SessionsUtilisateur");

            migrationBuilder.DropIndex(
                name: "IX_Prets_CompteDebitId1",
                table: "Prets");

            migrationBuilder.DropIndex(
                name: "IX_Prets_ValideParId1",
                table: "Prets");

            migrationBuilder.DropIndex(
                name: "IX_Parametres_AgenceId1",
                table: "Parametres");

            migrationBuilder.DropIndex(
                name: "IX_Comptes_ClientId1",
                table: "Comptes");

            migrationBuilder.DropIndex(
                name: "IX_Cartes_CompteId1",
                table: "Cartes");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_AgenceId1",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UtilisateurId1",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "VerificationsKYC");

            migrationBuilder.DropColumn(
                name: "VerifieParId1",
                table: "VerificationsKYC");

            migrationBuilder.DropColumn(
                name: "UtilisateurId1",
                table: "SessionsUtilisateur");

            migrationBuilder.DropColumn(
                name: "CompteDebitId1",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "ValideParId1",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "AgenceId1",
                table: "Parametres");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Comptes");

            migrationBuilder.DropColumn(
                name: "CompteId1",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "AgenceId1",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "UtilisateurId1",
                table: "AuditLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "VerificationsKYC",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifieParId1",
                table: "VerificationsKYC",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId1",
                table: "SessionsUtilisateur",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompteDebitId1",
                table: "Prets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValideParId1",
                table: "Prets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgenceId1",
                table: "Parametres",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "Comptes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompteId1",
                table: "Cartes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgenceId1",
                table: "AuditLog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId1",
                table: "AuditLog",
                type: "int",
                nullable: true);
migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_ClientId1",
                table: "VerificationsKYC",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_VerifieParId1",
                table: "VerificationsKYC",
                column: "VerifieParId1");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_UtilisateurId1",
                table: "SessionsUtilisateur",
                column: "UtilisateurId1");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_CompteDebitId1",
                table: "Prets",
                column: "CompteDebitId1");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ValideParId1",
                table: "Prets",
                column: "ValideParId1");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_AgenceId1",
                table: "Parametres",
                column: "AgenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_ClientId1",
                table: "Comptes",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_CompteId1",
                table: "Cartes",
                column: "CompteId1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_AgenceId1",
                table: "AuditLog",
                column: "AgenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UtilisateurId1",
                table: "AuditLog",
                column: "UtilisateurId1");
migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Agences_AgenceId1",
                table: "AuditLog",
                column: "AgenceId1",
                principalTable: "Agences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Utilisateur_UtilisateurId1",
                table: "AuditLog",
                column: "UtilisateurId1",
                principalTable: "Utilisateur",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cartes_Comptes_CompteId1",
                table: "Cartes",
                column: "CompteId1",
                principalTable: "Comptes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comptes_Clients_ClientId1",
                table: "Comptes",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Parametres_Agences_AgenceId1",
                table: "Parametres",
                column: "AgenceId1",
                principalTable: "Agences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prets_Comptes_CompteDebitId1",
                table: "Prets",
                column: "CompteDebitId1",
                principalTable: "Comptes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prets_Utilisateur_ValideParId1",
                table: "Prets",
                column: "ValideParId1",
                principalTable: "Utilisateur",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionsUtilisateur_Utilisateur_UtilisateurId1",
                table: "SessionsUtilisateur",
                column: "UtilisateurId1",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationsKYC_Clients_ClientId1",
                table: "VerificationsKYC",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationsKYC_Utilisateur_VerifieParId1",
                table: "VerificationsKYC",
                column: "VerifieParId1",
                principalTable: "Utilisateur",
                principalColumn: "Id");
        }
    }
}
