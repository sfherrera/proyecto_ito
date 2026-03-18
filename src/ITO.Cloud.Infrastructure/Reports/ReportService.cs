using ClosedXML.Excel;
using ITO.Cloud.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ITO.Cloud.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
    static ReportService()
    {
        // Licencia comunitaria (proyectos open-source / desarrollo)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── PDF ──────────────────────────────────────────────────────────────────
    public byte[] GenerateInspectionPdf(InspectionReportData data)
    {
        var doc = new InspectionPdfDocument(data);
        return doc.GeneratePdf();
    }

    // ── Excel ────────────────────────────────────────────────────────────────
    public byte[] GenerateObservationsExcel(ObservationsReportData data)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Observaciones");

        // ── Estilos base ─────────────────────────────────────────────────────
        var headerFill  = XLColor.FromHtml("#1565C0");
        var headerFont  = XLColor.White;
        var critBg      = XLColor.FromHtml("#FFCDD2");
        var altBg       = XLColor.FromHtml("#EEF2FF");
        var overdueFont = XLColor.FromHtml("#C62828");

        // ── Cabecera del reporte ─────────────────────────────────────────────
        ws.Cell("A1").Value = "ITO Cloud — Listado de Observaciones";
        ws.Cell("A1").Style.Font.Bold    = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1565C0");

        ws.Cell("A2").Value = $"Proyecto: {data.ProjectCode} — {data.ProjectName}";
        ws.Cell("A3").Value = $"Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm}";
        ws.Cell("A3").Style.Font.Italic = true;
        ws.Cell("A3").Style.Font.FontColor = XLColor.Gray;

        // ── Encabezados tabla ────────────────────────────────────────────────
        int headerRow = 5;
        string[] headers = ["Código", "Título", "Severidad", "Estado", "Contratista",
                             "Vencimiento", "Vencida", "Fecha Cierre"];

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Fill.BackgroundColor = headerFill;
            cell.Style.Font.FontColor       = headerFont;
            cell.Style.Font.Bold            = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // ── Filas de datos ───────────────────────────────────────────────────
        for (int r = 0; r < data.Observations.Count; r++)
        {
            var obs    = data.Observations[r];
            int wsRow  = headerRow + 1 + r;
            bool isAlt = r % 2 == 1;

            void SetCell(int col, object? val)
            {
                var cell = ws.Cell(wsRow, col);
                if (val != null) cell.Value = XLCellValue.FromObject(val);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Hair;

                if (obs.IsOverdue)
                    cell.Style.Font.FontColor = overdueFont;
                else if (obs.Severity is "Critica" or "Critical")
                    cell.Style.Fill.BackgroundColor = critBg;
                else if (isAlt)
                    cell.Style.Fill.BackgroundColor = altBg;
            }

            SetCell(1, obs.Code);
            SetCell(2, obs.Title);
            SetCell(3, obs.Severity);
            SetCell(4, obs.Status);
            SetCell(5, obs.ContractorName ?? "—");
            SetCell(6, obs.DueDate?.ToString("dd/MM/yyyy") ?? "—");
            SetCell(7, obs.IsOverdue ? "Sí" : "No");
            SetCell(8, obs.ClosedAt ?? "—");
        }

        // ── Resumen ───────────────────────────────────────────────────────────
        int summaryRow = headerRow + data.Observations.Count + 3;
        ws.Cell(summaryRow, 1).Value = "Resumen";
        ws.Cell(summaryRow, 1).Style.Font.Bold = true;

        ws.Cell(summaryRow + 1, 1).Value = "Total observaciones";
        ws.Cell(summaryRow + 1, 2).Value = data.Observations.Count;

        ws.Cell(summaryRow + 2, 1).Value = "Críticas";
        ws.Cell(summaryRow + 2, 2).Value = data.Observations.Count(o => o.Severity is "Critica" or "Critical");

        ws.Cell(summaryRow + 3, 1).Value = "Vencidas";
        ws.Cell(summaryRow + 3, 2).Value = data.Observations.Count(o => o.IsOverdue);

        ws.Cell(summaryRow + 4, 1).Value = "Cerradas";
        ws.Cell(summaryRow + 4, 2).Value = data.Observations.Count(o => o.Status == "Cerrada");

        // ── Ajuste de columnas ────────────────────────────────────────────────
        ws.Columns().AdjustToContents();
        ws.Column(2).Width = Math.Min(ws.Column(2).Width, 50); // Título máx 50

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
