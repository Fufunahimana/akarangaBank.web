using System.Globalization;
using AkarangaBank.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AkarangaBank.Web.Services;

public sealed record StatementPdfEntry(
    DateTime Date,
    string Reference,
    string Description,
    decimal Debit,
    decimal Credit);

public sealed record LoanSchedulePdfEntry(
    int Number,
    DateTime DueDate,
    decimal Principal,
    decimal Interest,
    decimal Total,
    string Status);

public static class PdfReportBuilder
{
    private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
    private static readonly string Navy = Colors.BlueGrey.Darken4;
    private static readonly string Gold = Colors.Amber.Darken2;
    private static readonly string Muted = Colors.Grey.Darken2;

    public static byte[] BuildAuditReport(IEnumerable<string> lines) => BuildTextDocument(
        "RAPPORT D'AUDIT",
        lines);

    public static byte[] BuildAccountStatement(
        string clientName,
        string clientNumber,
        string accountNumber,
        string accountType,
        string agency,
        string currency,
        decimal openingBalance,
        decimal closingBalance,
        DateTime generatedAt,
        IReadOnlyList<StatementPdfEntry> entries)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(8).FontColor(Colors.Grey.Darken4));
            AddHeader(page, "RELEVÉ DE COMPTE");
            page.Content().Column(column =>
            {
                column.Spacing(10);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(details =>
                    {
                        details.Item().Text($"Client : {clientName} ({clientNumber})");
                        details.Item().Text($"Compte : {accountNumber} · {accountType}");
                        details.Item().Text($"Agence : {agency}");
                    });
                    row.ConstantItem(185).AlignRight().Column(details =>
                    {
                        details.Item().Text($"Devise : {currency}");
                        details.Item().Text($"Édité le : {LocalDate(generatedAt)}").FontColor(Muted);
                    });
                });
                column.Item().Background(Colors.BlueGrey.Lighten5).BorderLeft(3).BorderColor(Gold).Padding(9)
                    .Text($"Solde initial : {Money(openingBalance)} {currency}     ·     Solde final : {Money(closingBalance)} {currency}")
                    .Bold().FontColor(Navy);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(92);
                        columns.ConstantColumn(96);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(82);
                        columns.ConstantColumn(82);
                        columns.ConstantColumn(88);
                    });
                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Date");
                        HeaderCell(header.Cell(), "Référence");
                        HeaderCell(header.Cell(), "Libellé");
                        HeaderCell(header.Cell(), "Débit");
                        HeaderCell(header.Cell(), "Crédit");
                        HeaderCell(header.Cell(), "Solde");
                    });

                    var runningBalance = openingBalance;
                    foreach (var entry in entries)
                    {
                        runningBalance += entry.Credit - entry.Debit;
                        BodyCell(table.Cell(), LocalDate(entry.Date));
                        BodyCell(table.Cell(), entry.Reference);
                        BodyCell(table.Cell(), entry.Description);
                        BodyCell(table.Cell(), entry.Debit == 0m ? "—" : Money(entry.Debit), rightAligned: true);
                        BodyCell(table.Cell(), entry.Credit == 0m ? "—" : Money(entry.Credit), rightAligned: true);
                        BodyCell(table.Cell(), Money(runningBalance), rightAligned: true);
                    }
                });
                column.Item().AlignRight().Text($"Solde de clôture : {Money(closingBalance)} {currency}").Bold().FontColor(Navy);
            });
            AddFooter(page);
        })).GeneratePdf();
    }

    public static byte[] BuildBalanceCertificate(
        string clientName,
        string clientNumber,
        string accountNumber,
        string accountType,
        string agency,
        decimal balance,
        string currency,
        DateTime issuedAt)
    {
        return CreateOfficialDocument("ATTESTATION DE SOLDE", column =>
        {
            column.Spacing(16);
            column.Item().Text("À QUI DE DROIT,").Bold().FontColor(Navy);
            column.Item().Text($"Akaranga Bank atteste que le compte {accountNumber}, ouvert au nom de {clientName} ({clientNumber}), présente à la date du {LocalDate(issuedAt)} le solde ci-dessous.")
                .FontSize(11).LineHeight(1.5f);
            column.Item().Background(Colors.BlueGrey.Lighten5).Border(1).BorderColor(Gold).Padding(18).Column(amount =>
            {
                amount.Item().AlignCenter().Text($"{Money(balance)} {currency}").FontSize(18).Bold().FontColor(Navy);
                amount.Item().AlignCenter().Text(AmountInWords(balance, currency)).Italic().FontColor(Muted);
            });
            column.Item().Text($"Compte : {accountNumber} · {accountType} · {agency}").FontColor(Muted);
            column.Item().Text("La présente attestation est délivrée à la demande de l'intéressé pour servir et valoir ce que de droit.");
            AddSignatureBlock(column, "Pour Akaranga Bank", LocalDate(issuedAt));
        });
    }

    public static byte[] BuildLoanContract(
        string reference,
        string clientName,
        string clientNumber,
        string clientIdentity,
        string product,
        string repaymentAccount,
        decimal principal,
        string currency,
        decimal annualRate,
        int termMonths,
        decimal installment,
        string purpose,
        DateTime approvedAt,
        IReadOnlyList<LoanSchedulePdfEntry> schedule)
    {
        return CreateOfficialDocument("CONTRAT DE PRÊT", column =>
        {
            column.Spacing(8);
            column.Item().Text($"Référence {reference} · conclu le {LocalDate(approvedAt)}").FontColor(Muted);
            AddSectionTitle(column, "PARTIES AU CONTRAT");
            column.Item().Text("Le prêteur : Akaranga Bank, Bujumbura, Burundi.");
            column.Item().Text($"L'emprunteur : {clientName}, client n° {clientNumber}, pièce d'identité n° {clientIdentity}.");

            AddArticle(column, "Article 1 — Objet", string.IsNullOrWhiteSpace(purpose)
                ? $"La banque consent un prêt de type « {product} » destiné à financer l'objet déclaré par l'emprunteur."
                : $"La banque consent un prêt de type « {product} » destiné à financer : {purpose}.");
            AddArticle(column, "Article 2 — Montant et durée", $"Le principal est de {Money(principal)} {currency}. La durée convenue est de {termMonths} mois à compter du déblocage.");
            AddArticle(column, "Article 3 — Taux", $"Le taux d'intérêt nominal annuel fixe est de {annualRate:N2} %, selon les conditions validées par la banque.");
            AddArticle(column, "Article 4 — Remboursement", $"L'emprunteur rembourse le prêt par {termMonths} échéances mensuelles calculées selon l'échéancier ci-dessous. La mensualité indicative est de {Money(installment)} {currency}. Le compte de remboursement est {repaymentAccount}.");
            AddArticle(column, "Article 5 — Garanties", "Les garanties et sûretés convenues entre les parties doivent être décrites dans le dossier de crédit et annexées au présent contrat avant signature. Aucune garantie non renseignée dans le dossier n'est réputée constituée par ce document.");
            AddArticle(column, "Article 6 — Retard et défaut", "Tout retard de paiement peut entraîner les pénalités prévues par le contrat et les textes applicables. En cas de défaut persistant, la banque peut engager les démarches de recouvrement prévues par la réglementation et les garanties valablement constituées.");

            AddSectionTitle(column, "ÉCHÉANCIER DE REMBOURSEMENT");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(42);
                    columns.ConstantColumn(78);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(62);
                });
                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "N°");
                    HeaderCell(header.Cell(), "Échéance");
                    HeaderCell(header.Cell(), "Capital");
                    HeaderCell(header.Cell(), "Intérêts");
                    HeaderCell(header.Cell(), "Total");
                    HeaderCell(header.Cell(), "Statut");
                });
                foreach (var item in schedule)
                {
                    BodyCell(table.Cell(), item.Number.ToString(CultureInfo.InvariantCulture));
                    BodyCell(table.Cell(), LocalDate(item.DueDate));
                    BodyCell(table.Cell(), Money(item.Principal), rightAligned: true);
                    BodyCell(table.Cell(), Money(item.Interest), rightAligned: true);
                    BodyCell(table.Cell(), Money(item.Total), rightAligned: true);
                    BodyCell(table.Cell(), item.Status);
                }
            });
            column.Item().Text("Les parties reconnaissent avoir pris connaissance des clauses ci-dessus et de l'échéancier.").FontColor(Muted);
            AddSignatureBlock(column, "Akaranga Bank", LocalDate(approvedAt), borrower: clientName);
        });
    }

    public static byte[] BuildCnrfDeclaration(
        string reference,
        DateTime createdAt,
        string clientName,
        string clientNumber,
        string identityNumber,
        string phone,
        string? operationReference,
        string? operationType,
        decimal? operationAmount,
        string? operationCurrency,
        DateTime? operationDate,
        string alertType,
        string alertLevel,
        string reason)
    {
        return CreateOfficialDocument("DÉCLARATION D'OPÉRATION SUSPECTE — CNRF", column =>
        {
            column.Spacing(10);
            column.Item().Background(Colors.BlueGrey.Lighten5).Padding(9).Text($"Référence : {reference} · Générée le {LocalDate(createdAt)}").Bold().FontColor(Navy);
            AddSectionTitle(column, "IDENTIFICATION DU CLIENT");
            AddKeyValue(column, "Nom", clientName);
            AddKeyValue(column, "Numéro client", clientNumber);
            AddKeyValue(column, "Pièce d'identité", identityNumber);
            AddKeyValue(column, "Téléphone", phone);
            AddSectionTitle(column, "OPÉRATION CONCERNÉE");
            AddKeyValue(column, "Référence", operationReference ?? "Aucune opération directement associée");
            AddKeyValue(column, "Type", operationType ?? "—");
            AddKeyValue(column, "Date", operationDate.HasValue ? LocalDate(operationDate.Value) : "—");
            AddKeyValue(column, "Montant", operationAmount.HasValue ? $"{Money(operationAmount.Value)} {operationCurrency}" : "—");
            AddSectionTitle(column, "MOTIF DE LA DÉCLARATION");
            AddKeyValue(column, "Type d'alerte", alertType);
            AddKeyValue(column, "Niveau", alertLevel);
            column.Item().Text(reason);
            AddSectionTitle(column, "MENTIONS LÉGALES ET CONFIDENTIALITÉ");
            column.Item().Text("Document confidentiel destiné à la Cellule Nationale de Renseignement Financier (CNRF) et aux personnes légalement habilitées. Il doit être revu par le responsable conformité et transmis par un canal officiel sécurisé conformément aux textes nationaux en vigueur et aux procédures internes de la banque.").LineHeight(1.35f);
            column.Item().Text("La génération de ce document ne vaut pas confirmation de sa transmission à la CNRF.").Bold().FontColor(Gold);
            AddSignatureBlock(column, "Responsable conformité habilité", LocalDate(createdAt));
        });
    }

    public static byte[] BuildOperationReceipt(
        string reference,
        string type,
        string status,
        string clientName,
        string accountNumber,
        decimal amount,
        string currency,
        DateTime operationDate,
        IReadOnlyList<(string Side, string Account, string Label, decimal Amount)> entries)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(25);
            page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(9).FontColor(Colors.Grey.Darken4));
            AddHeader(page, "REÇU D'OPÉRATION");
            page.Content().Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Copie client").FontColor(Gold).Bold();
                AddKeyValue(column, "Référence", reference);
                AddKeyValue(column, "Opération", type);
                AddKeyValue(column, "Date", LocalDate(operationDate));
                AddKeyValue(column, "Client", clientName);
                AddKeyValue(column, "Compte", accountNumber);
                AddKeyValue(column, "Montant", $"{Money(amount)} {currency}");
                AddKeyValue(column, "Statut", status);
                column.Item().PaddingTop(4).Text("DÉTAIL COMPTABLE").FontColor(Navy).Bold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(48);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(77);
                    });
                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Sens");
                        HeaderCell(header.Cell(), "Compte");
                        HeaderCell(header.Cell(), "Libellé");
                        HeaderCell(header.Cell(), "Montant");
                    });
                    foreach (var entry in entries)
                    {
                        BodyCell(table.Cell(), entry.Side);
                        BodyCell(table.Cell(), entry.Account);
                        BodyCell(table.Cell(), entry.Label);
                        BodyCell(table.Cell(), Money(entry.Amount), rightAligned: true);
                    }
                });
                column.Item().PaddingTop(8).Text("Conservez ce reçu pour vos dossiers.").FontColor(Muted).Italic();
            });
            AddFooter(page);
        })).GeneratePdf();
    }

    private static byte[] BuildTextDocument(string title, IEnumerable<string> lines) =>
        CreateOfficialDocument(title, column =>
        {
            column.Spacing(5);
            foreach (var line in lines)
                column.Item().Text(string.IsNullOrWhiteSpace(line) ? " " : line).FontSize(9);
        });

    private static byte[] CreateOfficialDocument(string title, Action<ColumnDescriptor> compose)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(38);
            page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(10).FontColor(Colors.Grey.Darken4));
            AddHeader(page, title);
            page.Content().Column(compose);
            AddFooter(page);
        })).GeneratePdf();
    }

    private static void AddHeader(PageDescriptor page, string title)
    {
        page.Header().Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(identity =>
                {
                    identity.Item().Text("AKARANGA BANK").FontSize(16).Bold().FontColor(Navy);
                    identity.Item().Text("Banque · Bujumbura, Burundi").FontSize(8).FontColor(Muted);
                });
                row.ConstantItem(94).AlignRight().Text("DOCUMENT OFFICIEL").FontSize(8).Bold().FontColor(Gold);
            });
            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(Gold);
            column.Item().PaddingTop(8).Text(title).FontSize(14).Bold().FontColor(Navy);
        });
    }

    private static void AddFooter(PageDescriptor page)
    {
        page.Footer().PaddingTop(8).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(text =>
        {
            text.Span("Akaranga Bank · Confidentiel · Page ").FontSize(8).FontColor(Muted);
            text.CurrentPageNumber().FontSize(8).FontColor(Muted);
            text.Span(" / ").FontSize(8).FontColor(Muted);
            text.TotalPages().FontSize(8).FontColor(Muted);
        });
    }

    private static void HeaderCell(IContainer container, string value) =>
        container.Background(Navy).BorderBottom(1).BorderColor(Gold).PaddingVertical(6).PaddingHorizontal(4)
            .Text(value).Bold().FontSize(8).FontColor(Colors.White);

    private static void BodyCell(IContainer container, string value, bool rightAligned = false)
    {
        var cell = container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(4);
        var text = rightAligned ? cell.AlignRight().Text(value) : cell.Text(value);
        text.FontSize(7.5f);
    }

    private static void AddSectionTitle(ColumnDescriptor column, string title) =>
        column.Item().PaddingTop(4).BorderBottom(1).BorderColor(Gold).PaddingBottom(4)
            .Text(title).Bold().FontColor(Navy);

    private static void AddArticle(ColumnDescriptor column, string title, string text)
    {
        column.Item().Column(article =>
        {
            article.Spacing(2);
            article.Item().Text(title).Bold().FontColor(Navy);
            article.Item().Text(text).LineHeight(1.25f);
        });
    }

    private static void AddKeyValue(ColumnDescriptor column, string label, string value) =>
        column.Item().Row(row =>
        {
            row.ConstantItem(120).Text(label).Bold().FontColor(Muted);
            row.RelativeItem().Text(value);
        });

    private static void AddSignatureBlock(
        ColumnDescriptor column,
        string bankSignatory,
        string date,
        string? borrower = null)
    {
        column.Item().PaddingTop(26).Row(row =>
        {
            row.RelativeItem().Column(signature =>
            {
                signature.Item().Text($"Fait à Bujumbura, le {date}");
                signature.Item().PaddingTop(24).Text(bankSignatory).Bold().FontColor(Navy);
                signature.Item().PaddingTop(34).Text("Signature et cachet").FontColor(Muted);
            });
            if (!string.IsNullOrWhiteSpace(borrower))
            {
                row.RelativeItem().Column(signature =>
                {
                    signature.Item().Text("L'emprunteur");
                    signature.Item().PaddingTop(24).Text(borrower).Bold().FontColor(Navy);
                    signature.Item().PaddingTop(34).Text("Signature").FontColor(Muted);
                });
            }
        });
    }

    private static string Money(decimal value) => value.ToString("N2", FrenchCulture);

    private static string LocalDate(DateTime value) =>
        BankTime.ToLocal(value).ToString("dd/MM/yyyy HH:mm", FrenchCulture);

    private static string AmountInWords(decimal amount, string currency)
    {
        var rounded = decimal.Round(Math.Abs(amount), 2, MidpointRounding.AwayFromZero);
        var whole = decimal.ToInt64(decimal.Truncate(rounded));
        var cents = (int)((rounded - whole) * 100m);
        var unit = currency.Equals("BIF", StringComparison.OrdinalIgnoreCase)
            ? "franc burundais"
            : currency;
        var text = $"{FrenchNumberToWords(whole)} {unit}{(whole > 1 ? "s" : string.Empty)}";
        if (cents > 0)
            text += $" et {FrenchNumberToWords(cents)} centime{(cents > 1 ? "s" : string.Empty)}";
        return amount < 0 ? $"moins {text}" : text;
    }

    private static string FrenchNumberToWords(long value)
    {
        if (value == 0)
            return "zéro";
        if (value < 0)
            return $"moins {FrenchNumberToWords(Math.Abs(value))}";
        if (value >= 1_000_000_000_000)
            return value.ToString("N0", FrenchCulture);

        var parts = new List<string>();
        var billions = value / 1_000_000_000;
        var millions = value % 1_000_000_000 / 1_000_000;
        var thousands = value % 1_000_000 / 1_000;
        var remainder = (int)(value % 1_000);
        if (billions > 0)
            parts.Add($"{FrenchNumberToWords(billions)} milliard{(billions > 1 ? "s" : string.Empty)}");
        if (millions > 0)
            parts.Add($"{FrenchNumberToWords(millions)} million{(millions > 1 ? "s" : string.Empty)}");
        if (thousands > 0)
            parts.Add(thousands == 1 ? "mille" : $"{FrenchNumberToWords(thousands)} mille");
        if (remainder > 0)
            parts.Add(BelowThousand(remainder));
        return string.Join(" ", parts);
    }

    private static string BelowThousand(int value)
    {
        if (value < 100)
            return BelowHundred(value);
        var hundreds = value / 100;
        var remainder = value % 100;
        var prefix = hundreds == 1 ? "cent" : $"{BelowHundred(hundreds)} cent";
        if (remainder == 0 && hundreds > 1)
            prefix += "s";
        return remainder == 0 ? prefix : $"{prefix} {BelowHundred(remainder)}";
    }

    private static string BelowHundred(int value)
    {
        string[] units = ["zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize"];
        if (value < units.Length)
            return units[value];
        if (value < 20)
            return $"dix-{units[value - 10]}";
        if (value < 70)
        {
            var tens = value / 10;
            var remainder = value % 10;
            var tensText = new[] { "", "", "vingt", "trente", "quarante", "cinquante", "soixante" }[tens];
            return remainder switch
            {
                0 => tensText,
                1 => $"{tensText} et un",
                _ => $"{tensText}-{units[remainder]}"
            };
        }
        if (value < 80)
            return value == 71 ? "soixante et onze" : $"soixante-{BelowHundred(value - 60)}";
        if (value < 100)
            return value == 80 ? "quatre-vingts" : $"quatre-vingt-{BelowHundred(value - 80)}";
        throw new ArgumentOutOfRangeException(nameof(value));
    }
}
