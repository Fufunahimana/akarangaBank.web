using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class LinkBankOperationsToIdentityUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertesAML_Utilisateur_TraiteParId",
                table: "AlertesAML");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Utilisateur_UtilisateurId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Virements_Utilisateur_UtilisateurId",
                table: "Virements");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertesAML_Utilisateurs_TraiteParId",
                table: "AlertesAML",
                column: "TraiteParId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Utilisateurs_UtilisateurId",
                table: "Operations",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Virements_Utilisateurs_UtilisateurId",
                table: "Virements",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertesAML_Utilisateurs_TraiteParId",
                table: "AlertesAML");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Utilisateurs_UtilisateurId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Virements_Utilisateurs_UtilisateurId",
                table: "Virements");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertesAML_Utilisateur_TraiteParId",
                table: "AlertesAML",
                column: "TraiteParId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Utilisateur_UtilisateurId",
                table: "Operations",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Virements_Utilisateur_UtilisateurId",
                table: "Virements",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
