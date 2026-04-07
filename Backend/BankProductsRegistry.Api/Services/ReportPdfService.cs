using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankProductsRegistry.Api.Services;

public sealed class ReportPdfService : IReportPdfService
{
    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(4);

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
                    c.Item().Text("Registro de productos bancarios").FontSize(8).FontColor(Colors.Grey.Medium);
                    c.Item().Text("Reporte de portafolio").Bold().FontSize(16);
                    c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Cliente: {dto.ClientName}").SemiBold();
                    col.Item().Text($"Correo: {dto.Email}");
                    col.Item().Text(
                        $"Productos: {dto.TotalProducts}  |  Balance actual: DOP {dto.CurrentBalance:N2}  |  Depósitos: DOP {dto.TotalDeposits:N2}  |  Retiros: DOP {dto.TotalWithdrawals:N2}");
                    col.Item().PaddingTop(8).Text("Cuentas").SemiBold().FontSize(11);
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
                            h.Cell().Element(CellStyle).Text("Cuenta").SemiBold();
                            h.Cell().Element(CellStyle).Text("Producto").SemiBold();
                            h.Cell().Element(CellStyle).Text("Estado").SemiBold();
                            h.Cell().Element(CellStyle).Text("Balance").SemiBold();
                            h.Cell().Element(CellStyle).Text("Apertura").SemiBold();
                            h.Cell().Element(CellStyle).Text("Movs.").SemiBold();
                            h.Cell().Element(CellStyle).Text("Dep / Ret").SemiBold();
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
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8).SemiBold();
                });
            });
        }).GeneratePdf();

    public byte[] BuildCreditHistoryPdf(ClientCreditHistoryReportDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9f));
                page.Header().Column(c =>
                {
                    c.Item().Text("Historial crediticio interno").Bold().FontSize(15);
                    c.Item().Text($"{dto.ClientName} · ID {dto.ClientId}").FontSize(10);
                    c.Item().Text($"Cédula: {dto.NationalId}  ·  {dto.Email}").FontSize(8).FontColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Text("Resumen").SemiBold().FontSize(11);
                    col.Item().Text(
                        $"Productos: {dto.Overview.TotalProducts} activos {dto.Overview.ActiveProducts} · " +
                        $"Exposición DOP {dto.Overview.CurrentCreditExposure:N2} · Límite aprobado DOP {dto.Overview.ApprovedCreditLimit:N2}");
                    col.Item().Text(
                        $"Mora: {dto.Overview.DelinquentProducts} · Bloqueos activos: {dto.Overview.ActiveBlockedProducts} · Eventos fraude: {dto.Overview.FraudBlockEvents}");
                    col.Item().PaddingTop(6).Text("Score interno (referencia)").SemiBold();
                    col.Item().Text($"{dto.Score.Score} — {dto.Score.RiskBand} · Utilización {(dto.Score.CreditUtilizationRatio ?? 0m):P1}");
                    col.Item().PaddingTop(8).Text("Productos").SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.1f);
                            c.RelativeColumn(1.3f);
                            c.RelativeColumn(0.8f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(0.9f);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Cuenta").SemiBold();
                            h.Cell().Element(CellStyle).Text("Producto").SemiBold();
                            h.Cell().Element(CellStyle).Text("Tipo").SemiBold();
                            h.Cell().Element(CellStyle).Text("Estado").SemiBold();
                            h.Cell().Element(CellStyle).Text("Balance").SemiBold();
                        });
                        foreach (var a in dto.Accounts.Take(25))
                        {
                            t.Cell().Element(CellStyle).Text(a.AccountNumber);
                            t.Cell().Element(CellStyle).Text(a.ProductName);
                            t.Cell().Element(CellStyle).Text(a.ProductType.ToString());
                            t.Cell().Element(CellStyle).Text(a.Status.ToString());
                            t.Cell().Element(CellStyle).Text($"DOP {a.CurrentBalance:N2}");
                        }
                    });
                    col.Item().PaddingTop(8).Text("Eventos recientes").SemiBold();
                    foreach (var ev in dto.RecentEvents.Take(12))
                    {
                        col.Item().Text($"{ev.OccurredAt:dd/MM/yyyy HH:mm} [{ev.Severity}] {ev.Title}: {ev.Detail}")
                            .FontSize(8);
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generado ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(dto.GeneratedAt.ToString("dd/MM/yyyy HH:mm")).FontSize(8).SemiBold();
                    x.Span(" UTC").FontSize(8);
                });
            });
        }).GeneratePdf();

    public byte[] BuildCreditScorePdf(ClientCreditScoreReportDto dto) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Column(c =>
                {
                    c.Item().Text("Score crediticio interno").Bold().FontSize(16);
                    c.Item().Text(dto.ClientName).SemiBold();
                    c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingTop(14).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Puntuación: {dto.Score}").FontSize(18).SemiBold();
                    col.Item().Text($"Banda de riesgo: {dto.RiskBand}");
                    col.Item().Text($"Metodología: {dto.Methodology}").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text(dto.Disclaimer).FontSize(8).Italic();
                    col.Item().Text(
                        $"Utilización crédito: {(dto.CreditUtilizationRatio ?? 0m):P1} · Productos en mora: {dto.DelinquentProducts} · " +
                        $"Bloqueos: {dto.ActiveBlockedProducts} · Fraude: {dto.FraudBlockEvents}");
                    col.Item().PaddingTop(10).Text("Factores").SemiBold();
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
                            h.Cell().Element(CellStyle).Text("Factor").SemiBold();
                            h.Cell().Element(CellStyle).Text("Impacto").SemiBold();
                            h.Cell().Element(CellStyle).Text("Detalle").SemiBold();
                        });
                        foreach (var f in dto.Factors)
                        {
                            t.Cell().Element(CellStyle).Text(f.Factor);
                            t.Cell().Element(CellStyle).Text(f.Impact.ToString());
                            t.Cell().Element(CellStyle).Text(f.Detail);
                        }
                    });
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generado ").FontSize(8);
                    x.Span(dto.GeneratedAt.ToString("dd/MM/yyyy HH:mm") + " UTC").SemiBold().FontSize(8);
                });
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
                    c.Item().Text("Dashboard — resumen general").Bold().FontSize(16);
                    c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Medium);
                });
                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Clientes activos: {dto.TotalClients}");
                    col.Item().Text($"Productos activos: {dto.ActiveProducts}");
                    col.Item().Text($"Transacciones registradas: {dto.TotalTransactions}");
                    col.Item().Text($"Volumen total movido: DOP {dto.TotalVolume:N2}");
                    col.Item().PaddingTop(8).Text("Últimas transacciones").SemiBold();
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
                            h.Cell().Element(CellStyle).Text("Cliente").SemiBold();
                            h.Cell().Element(CellStyle).Text("Producto").SemiBold();
                            h.Cell().Element(CellStyle).Text("Tipo").SemiBold();
                            h.Cell().Element(CellStyle).Text("Monto").SemiBold();
                            h.Cell().Element(CellStyle).Text("Fecha").SemiBold();
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
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generado ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC").FontSize(8).SemiBold();
                });
            });
        }).GeneratePdf();
}
