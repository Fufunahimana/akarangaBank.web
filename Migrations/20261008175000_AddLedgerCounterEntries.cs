using AkarangaBank.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations;

public partial class AddLedgerCounterEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CodeCompteGeneral",
            table: "Ecritures",
            type: "varchar(50)",
            maxLength: 50,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<int?>(
            name: "CompteId",
            table: "Ecritures",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.Sql("""
            UPDATE Ecritures AS e
            INNER JOIN Operations AS o ON o.Id = e.OperationId
            LEFT JOIN Comptes AS c ON c.Id = e.CompteId
            SET e.CodeCompteGeneral = CASE
                WHEN c.Id IS NULL AND o.TypeOperation = 'Depot' THEN CONCAT('CAISSE:', o.AgenceId)
                WHEN c.Id IS NULL AND o.TypeOperation = 'Frais' THEN CONCAT('PRODUITS_FRAIS:', o.AgenceId)
                WHEN c.Id IS NOT NULL THEN CONCAT('CLIENT:', c.NumeroCompte)
                ELSE e.CodeCompteGeneral
            END
            WHERE e.CodeCompteGeneral IS NULL;
            """);

        migrationBuilder.Sql("""
            INSERT INTO Ecritures
                (OperationId, CompteId, CodeCompteGeneral, Sens, Montant, Devise, DateEcriture, Libelle)
            SELECT o.Id,
                   NULL,
                   CASE WHEN o.TypeOperation = 'Frais'
                        THEN CONCAT('PRODUITS_FRAIS:', o.AgenceId)
                        ELSE CONCAT('CAISSE:', o.AgenceId)
                   END,
                   CASE WHEN o.TypeOperation = 'Depot' THEN 'Debit' ELSE 'Credit' END,
                   o.Montant,
                   o.Devise,
                   o.DateOperation,
                   CONCAT('Contrepartie de ', o.TypeOperation, ' ', o.Reference)
            FROM Operations AS o
            WHERE o.TypeOperation IN ('Depot', 'Retrait', 'Frais')
              AND (SELECT COUNT(*) FROM Ecritures AS e WHERE e.OperationId = o.Id) = 1;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE e FROM Ecritures AS e
            INNER JOIN Operations AS o ON o.Id = e.OperationId
            WHERE e.CompteId IS NULL
              AND o.TypeOperation IN ('Depot', 'Retrait', 'Frais');
            """);

        migrationBuilder.DropColumn(
            name: "CodeCompteGeneral",
            table: "Ecritures");

        migrationBuilder.AlterColumn<int>(
            name: "CompteId",
            table: "Ecritures",
            type: "int",
            nullable: false,
            oldClrType: typeof(int?),
            oldType: "int",
            oldNullable: true);
    }
}
