using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankProductsRegistry.Api.Services;

public sealed class ReportPdfService : IReportPdfService
{
    /// <summary>Azul tipo encabezados de informe (similar a reportes de crédito).</summary>
    private static readonly Color SectionBarBlue = Color.FromRGB(79, 129, 189);

    /// <summary>Fondo suave para cabeceras de tabla.</summary>
    private static readonly Color TableHeaderFill = Color.FromRGB(217, 226, 243);

    private const string BrandName = "BankRED";

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(4);

    private static IContainer TableHeaderCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Background(TableHeaderFill).PaddingVertical(6)
            .PaddingHorizontal(4);

    private static IContainer SectionBar(IContainer container) =>
        container.Background(SectionBarBlue).PaddingVertical(7).PaddingHorizontal(10);

    private static void AppendFooter(PageDescriptor page, string? extraLine = null)
    {
        page.Footer().Row(row =>
        {
            row.RelativeItem().AlignLeft().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(Colors.Grey.Medium));
                text.Span(extraLine ?? $"{BrandName} · Documento interno de referencia");
            });
            row.AutoItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    public byte[] BuildPortfolioPdf(ClientPortfolioReportDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9.5f));
                page.Header().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{BrandName}").FontSize(8).FontColor(Colors.Grey.Medium);
                        r.AutoItem().Text(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                    c.Item().PaddingTop(4).Text("Reporte de portafolio").Bold().FontSize(17);
                    c.Item().PaddingTop(2).Text("Registro de productos bancarios").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Element(SectionBar).Text("Resumen del cliente").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().PaddingHorizontal(2).Column(inner =>
                    {
                        inner.Spacing(4);
                        inner.Item().Text($"Cliente: {dto.ClientName}").SemiBold();
                        inner.Item().Text($"Correo: {dto.Email}");
                        inner.Item().Text(
                            $"Productos: {dto.TotalProducts}  ·  Balance actual: DOP {dto.CurrentBalance:N2}  ·  Depósitos: DOP {dto.TotalDeposits:N2}  ·  Retiros: DOP {dto.TotalWithdrawals:N2}");
                    });

                    col.Item().Element(SectionBar).Text("Cuentas").FontColor(Colors.White).SemiBold().FontSize(10.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(1.2f);
                            cols.RelativeColumn(1.4f);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(0.9f);
                            cols.RelativeColumn(0.8f);
                            cols.RelativeColumn(0.8f);
                            cols.RelativeColumn(0.8f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(TableHeaderCell).Text("Cuenta").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Producto").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Estado").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Balance").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Apertura").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Movs.").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Dep / Ret").SemiBold();
                        });
                        foreach (var a in dto.Accounts)
                        {
                            t.Cell().Element(CellStyle).Text(a.AccountNumber);
                            t.Cell().Element(CellStyle).Text(a.ProductName);
                            t.Cell().Element(CellStyle).Text(a.Status.ToString());
                            t.Cell().Element(CellStyle).Text($"DOP {a.Amount:N2}");
                            t.Cell().Element(CellStyle).Text(a.OpenDate.ToString("dd/MM/yyyy"));
                            t.Cell().Element(CellStyle).Text(a.TotalTransactions.ToString());
                            t.Cell().Element(CellStyle).Text($"{a.Deposits:N0} / {a.Withdrawals:N0}");
                        }
                    });
                });
                AppendFooter(page);
            });
        }).GeneratePdf();

    public byte[] BuildCreditHistoryPdf(ClientCreditHistoryReportDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(8.5f));
                page.Header().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{BrandName}").FontSize(8).FontColor(Colors.Grey.Medium);
                        r.AutoItem().Text(dto.GeneratedAt.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                    c.Item().PaddingTop(6).Text("Tu Historia de Crédito").Bold().FontSize(18);
                    c.Item().PaddingTop(2).Text("Informe crediticio interno (referencia operativa)").FontSize(9)
                        .FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Element(SectionBar).Text("Datos personales").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(5);
                            CreditField(left, "Cédula", dto.NationalId);
                            CreditField(left, "Nombres y apellidos", dto.ClientName);
                            CreditField(left, "Correo electrónico", string.IsNullOrWhiteSpace(dto.Email) ? "—" : dto.Email);
                            CreditField(left, "Teléfono",
                                string.IsNullOrWhiteSpace(dto.Phone) ? "—" : dto.Phone);
                            CreditField(left, "ID cliente",
                                dto.ClientId.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        });
                        row.ConstantItem(120).AlignRight().AlignCenter().Column(score =>
                        {
                            score.Item().Text("Score interno").FontSize(8).FontColor(Colors.Grey.Medium);
                            score.Item().Text(dto.Score.Score.ToString()).FontSize(22).Bold().FontColor(SectionBarBlue);
                            score.Item().Text(dto.Score.RiskBand).FontSize(9).SemiBold();
                            score.Item().PaddingTop(4).Text("Utilización").FontSize(7).FontColor(Colors.Grey.Medium);
                            score.Item().Text($"{(dto.Score.CreditUtilizationRatio ?? 0m):P1}").FontSize(9);
                        });
                    });

                    col.Item().Element(SectionBar).Text("Leyenda de severidad de eventos").FontColor(Colors.White)
                        .SemiBold().FontSize(10.5f);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(
                        "info: operación normal  ·  warning: advertencia riesgo / mora  ·  error: bloqueo o incidencia grave  ·  —: historial no disponible para ese campo")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);

                    col.Item().Element(SectionBar).Text("Datos consolidados").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(ov =>
                    {
                        ov.Spacing(4);
                        ov.Item().Text(
                            $"Productos: {dto.Overview.TotalProducts} total · {dto.Overview.ActiveProducts} activos · " +
                            $"En mora: {dto.Overview.DelinquentProducts} · Bloqueos activos: {dto.Overview.ActiveBlockedProducts} · Eventos fraude: {dto.Overview.FraudBlockEvents}");
                        ov.Item().Text(
                            $"Exposición actual DOP {dto.Overview.CurrentCreditExposure:N2} · Límite aprobado DOP {dto.Overview.ApprovedCreditLimit:N2} · " +
                            $"Cargos DOP {dto.Overview.TotalCharges:N2} · Pagos DOP {dto.Overview.TotalPayments:N2}");
                        if (dto.Overview.OldestOpenDate.HasValue)
                        {
                            ov.Item().Text(
                                    $"Antigüedad portafolio: apertura más antigua {dto.Overview.OldestOpenDate:dd/MM/yyyy}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                    });

                    col.Item().Element(SectionBar).Text("Detalle de cuentas activas / vigentes").FontColor(Colors.White)
                        .SemiBold().FontSize(10.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.35f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(0.75f);
                            c.RelativeColumn(0.75f);
                            c.RelativeColumn(0.55f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(0.65f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(0.9f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(TableHeaderCell).Text("Producto / Cuenta").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Tipo").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Estado").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Apertura").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Mon.").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Límite").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Balance").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Util.").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Movs.").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Bloqueo").SemiBold();
                        });
                        foreach (var a in dto.Accounts.Take(25))
                        {
                            var productLine = $"{a.ProductName}\n{a.AccountNumber}";
                            if (productLine.Length > 80)
                            {
                                productLine = a.ProductName.Length > 40
                                    ? a.ProductName[..37] + "…"
                                    : a.ProductName;
                                productLine += $"\n{a.AccountNumber}";
                            }

                            t.Cell().Element(CellStyle).Text(productLine);
                            t.Cell().Element(CellStyle).Text(ShortProductType(a.ProductType));
                            t.Cell().Element(CellStyle).Text(a.Status.ToString());
                            t.Cell().Element(CellStyle).Text(a.OpenDate.ToString("dd/MM/yyyy"));
                            t.Cell().Element(CellStyle).Text("DOP");
                            t.Cell().Element(CellStyle).Text(a.ApprovedCreditLimit.HasValue
                                ? $"DOP {a.ApprovedCreditLimit:N2}"
                                : "—");
                            t.Cell().Element(CellStyle).Text($"DOP {a.CurrentBalance:N2}");
                            t.Cell().Element(CellStyle).Text(a.CreditUtilizationRatio.HasValue
                                ? $"{a.CreditUtilizationRatio:P0}"
                                : "—");
                            t.Cell().Element(CellStyle).Text(a.TotalTransactions.ToString());
                            t.Cell().Element(CellStyle).Text(BlockSummary(a));
                        }
                    });

                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(10)
                        .Row(tot =>
                        {
                            tot.RelativeItem().Text("TOTALES GENERALES (DOP)").SemiBold().FontSize(9);
                            tot.AutoItem().Text(
                                    $"Exposición: {dto.Overview.CurrentCreditExposure:N2}  ·  Límite: {dto.Overview.ApprovedCreditLimit:N2}")
                                .FontSize(9);
                        });

                    col.Item().Element(SectionBar).Text("Eventos recientes en el historial").FontColor(Colors.White)
                        .SemiBold().FontSize(10.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.1f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(1.1f);
                            c.RelativeColumn(2.2f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(TableHeaderCell).Text("Fecha / hora").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Severidad").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Evento").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Detalle").SemiBold();
                        });
                        foreach (var ev in dto.RecentEvents.Take(12))
                        {
                            t.Cell().Element(CellStyle).Text($"{ev.OccurredAt:dd/MM/yyyy HH:mm}");
                            t.Cell().Element(CellStyle).Text(ev.Severity);
                            t.Cell().Element(CellStyle).Text(ev.Title);
                            t.Cell().Element(CellStyle).Text($"[{ev.Category}] {ev.Detail}");
                        }
                    });

                    col.Item().PaddingTop(4).Text(
                            "Este informe resume el comportamiento interno en BankRED. No sustituye un reporte de Buró de Crédito ni de TransUnion.")
                        .FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                });
                AppendFooter(page);
            });
        }).GeneratePdf();

    private static void CreditField(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(110).Text(label + ":").FontSize(8).FontColor(Colors.Grey.Medium);
            r.RelativeItem().Text(value).SemiBold().FontSize(9);
        });
    }

    private static string ShortProductType(Models.Enums.ProductType type) =>
        type switch
        {
            Models.Enums.ProductType.SavingsAccount => "AH",
            Models.Enums.ProductType.CreditCard => "TC",
            Models.Enums.ProductType.Loan => "PR",
            Models.Enums.ProductType.Investment => "INV",
            Models.Enums.ProductType.Certificate => "CD",
            _ => type.ToString()
        };

    private static string BlockSummary(ClientCreditAccountHistoryItemDto a)
    {
        if (!a.IsBlocked)
        {
            return "No";
        }

        return a.ActiveBlockType?.ToString() ?? "Sí";
    }

    public byte[] BuildCreditScorePdf(ClientCreditScoreReportDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9.5f));
                page.Header().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{BrandName}").FontSize(8).FontColor(Colors.Grey.Medium);
                        r.AutoItem().Text(dto.GeneratedAt.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                    c.Item().PaddingTop(6).Text("Score crediticio interno").Bold().FontSize(18);
                    c.Item().PaddingTop(2).Text(dto.ClientName).SemiBold().FontSize(12);
                });
                page.Content().PaddingTop(14).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Element(SectionBar).Text("Resumen del score").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(16).Row(r =>
                    {
                        r.RelativeItem().Column(left =>
                        {
                            left.Item().Text($"Puntuación: {dto.Score}").FontSize(20).Bold().FontColor(SectionBarBlue);
                            left.Item().PaddingTop(6).Text($"Banda de riesgo: {dto.RiskBand}");
                            left.Item().PaddingTop(4).Text($"Metodología: {dto.Methodology}").FontSize(8)
                                .FontColor(Colors.Grey.Medium);
                        });
                        r.ConstantItem(200).Column(right =>
                        {
                            right.Item().Text("Indicadores").FontSize(8).FontColor(Colors.Grey.Medium);
                            right.Item().Text(
                                $"Utilización: {(dto.CreditUtilizationRatio ?? 0m):P1} · Mora: {dto.DelinquentProducts} · Bloqueos: {dto.ActiveBlockedProducts} · Fraude: {dto.FraudBlockEvents}");
                        });
                    });

                    col.Item().Text(dto.Disclaimer).FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

                    col.Item().Element(SectionBar).Text("Factores del modelo").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(0.5f);
                            c.RelativeColumn(2f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(TableHeaderCell).Text("Factor").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Impacto").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Detalle").SemiBold();
                        });
                        foreach (var f in dto.Factors)
                        {
                            t.Cell().Element(CellStyle).Text(f.Factor);
                            t.Cell().Element(CellStyle).Text(f.Impact.ToString());
                            t.Cell().Element(CellStyle).Text(f.Detail);
                        }
                    });
                });
                AppendFooter(page);
            });
        }).GeneratePdf();

    public byte[] BuildDashboardPdf(DashboardSummaryDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{BrandName}").FontSize(8).FontColor(Colors.Grey.Medium);
                        r.AutoItem().Text(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                    c.Item().PaddingTop(4).Text("Dashboard — resumen general").Bold().FontSize(17);
                });
                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Element(SectionBar).Text("Indicadores").FontColor(Colors.White).SemiBold().FontSize(10.5f);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(k =>
                    {
                        k.Spacing(6);
                        k.Item().Text($"Clientes activos: {dto.TotalClients}");
                        k.Item().Text($"Productos activos: {dto.ActiveProducts}");
                        k.Item().Text($"Transacciones registradas: {dto.TotalTransactions}");
                        k.Item().Text($"Volumen total movido: DOP {dto.TotalVolume:N2}");
                    });

                    col.Item().Element(SectionBar).Text("Últimas transacciones").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(0.7f);
                            c.RelativeColumn(0.9f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(TableHeaderCell).Text("Cliente").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Producto").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Tipo").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Monto").SemiBold();
                            h.Cell().Element(TableHeaderCell).Text("Fecha").SemiBold();
                        });
                        foreach (var r in dto.RecentTransactions)
                        {
                            t.Cell().Element(CellStyle).Text(r.ClientName);
                            t.Cell().Element(CellStyle).Text(r.ProductName);
                            t.Cell().Element(CellStyle).Text(r.TransactionType);
                            t.Cell().Element(CellStyle).Text($"DOP {r.Amount:N2}");
                            t.Cell().Element(CellStyle).Text(r.Date.ToString("dd/MM/yy HH:mm"));
                        }
                    });
                });
                AppendFooter(page);
            });
        }).GeneratePdf();

    public byte[] BuildTransactionStatementPdf(ClientTransactionStatementDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9f));
                page.Header().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"{BrandName}").FontSize(8).FontColor(Colors.Grey.Lighten4);
                        r.AutoItem().Text(dto.GeneratedAtUtc.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8)
                            .FontColor(Colors.Grey.Lighten4);
                    });
                    c.Item().Background(SectionBarBlue).Padding(14).Column(b =>
                    {
                        b.Item().Text("Estado de cuenta — movimientos").FontColor(Colors.White).Bold().FontSize(16);
                        b.Item().PaddingTop(2).Text(
                                $"Periodo: {dto.FromDate:dd/MM/yyyy} al {dto.ToDate:dd/MM/yyyy}")
                            .FontColor(Colors.Grey.Lighten4).FontSize(9);
                    });
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Element(SectionBar).Text("Titular y cuenta").FontColor(Colors.White).SemiBold()
                        .FontSize(10.5f);
                    col.Item().PaddingHorizontal(2).Column(inner =>
                    {
                        inner.Item().Text(dto.ClientName).SemiBold().FontSize(12);
                        if (!string.IsNullOrWhiteSpace(dto.Email))
                        {
                            inner.Item().Text($"Correo: {dto.Email}");
                        }
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    foreach (var account in dto.Accounts)
                    {
                        col.Item().PaddingTop(6).Text($"{account.ProductName}  ·  DOP {account.AccountNumber}")
                            .SemiBold().FontSize(11);

                        if (account.Rows.Count == 0)
                        {
                            col.Item().Text("Sin movimientos en el periodo seleccionado.").Italic()
                                .FontColor(Colors.Grey.Medium);
                            continue;
                        }

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(0.95f);
                                c.RelativeColumn(0.95f);
                                c.RelativeColumn(0.95f);
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(1f);
                                c.RelativeColumn(1f);
                            });

                            t.Header(h =>
                            {
                                h.Cell().Element(TableHeaderCell).Text("Fecha posteo").SemiBold();
                                h.Cell().Element(TableHeaderCell).Text("Fecha efectiva").SemiBold();
                                h.Cell().Element(TableHeaderCell).Text("Referencia").SemiBold();
                                h.Cell().Element(TableHeaderCell).Text("Descripción").SemiBold();
                                h.Cell().Element(TableHeaderCell).Text("Monto").SemiBold();
                                h.Cell().Element(TableHeaderCell).Text("Balance").SemiBold();
                            });

                            foreach (var row in account.Rows)
                            {
                                var desc = $"{row.TransactionTypeLabel}".Trim();
                                if (!string.IsNullOrWhiteSpace(row.Description))
                                {
                                    desc = $"{row.TransactionTypeLabel} — {row.Description}";
                                }

                                t.Cell().Element(CellStyle).Text(row.TransactionDate.ToString("dd/MM/yyyy"));
                                t.Cell().Element(CellStyle).Text(row.TransactionDate.ToString("dd/MM/yyyy"));
                                t.Cell().Element(CellStyle).Text(row.ReferenceNumber ?? "—");
                                t.Cell().Element(CellStyle).Text(desc);
                                t.Cell().Element(CellStyle).Text(FormatDopStatement(row.SignedAmount));
                                t.Cell().Element(CellStyle).Text(FormatDopStatement(row.BalanceAfter));
                            }
                        });
                    }
                });

                AppendFooter(page);
            });
        }).GeneratePdf();

    private static string FormatDopStatement(decimal value)
    {
        var abs = Math.Abs(value);
        var s = $"DOP {abs:N2}";
        return value < 0 ? $"{s}-" : s;
    }
}
