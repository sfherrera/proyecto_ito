using ITO.Cloud.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ITO.Cloud.Infrastructure.Reports;

/// <summary>
/// Documento PDF de inspección técnica generado con QuestPDF.
/// </summary>
internal sealed class InspectionPdfDocument : IDocument
{
    private readonly InspectionReportData _data;

    public InspectionPdfDocument(InspectionReportData data) => _data = data;

    public DocumentMetadata GetMetadata() => new()
    {
        Title        = $"Inspección {_data.InspectionCode}",
        Author       = "ITO Cloud",
        Subject      = "Informe de Inspección Técnica",
        CreationDate = DateTimeOffset.UtcNow
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ── Header ───────────────────────────────────────────────────────────────
    private void ComposeHeader(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("ITO Cloud").Bold().FontSize(16).FontColor("#1565C0");
                    left.Item().Text("Informe de Inspección Técnica").FontSize(11).FontColor("#555555");
                });
                row.ConstantItem(140).AlignRight().Column(right =>
                {
                    right.Item().Text(_data.InspectionCode).Bold().FontSize(14).FontColor("#1565C0");
                    right.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#888888");
                });
            });
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#1565C0");
        });
    }

    // ── Content ──────────────────────────────────────────────────────────────
    private void ComposeContent(IContainer c)
    {
        c.PaddingTop(8).Column(col =>
        {
            // Datos generales
            col.Item().Background("#EEF2FF").Padding(8).Column(info =>
            {
                info.Item().Text("Datos Generales").Bold().FontSize(10);
                info.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Column(left =>
                    {
                        AddLabelValue(left, "Proyecto",    _data.ProjectName);
                        AddLabelValue(left, "Código",      _data.ProjectCode);
                        AddLabelValue(left, "Inspector",   _data.InspectorName ?? "-");
                        AddLabelValue(left, "Título",      _data.InspectionTitle);
                    });
                    r.RelativeItem().Column(right =>
                    {
                        AddLabelValue(right, "Estado",       _data.Status);
                        AddLabelValue(right, "Programado",   _data.ScheduledDate?.ToString("dd/MM/yyyy") ?? "-");
                        AddLabelValue(right, "Finalizado",   _data.FinishedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        AddLabelValue(right, "Clima",        _data.WeatherConditions ?? "-");
                    });
                });
            });

            // Resultado / puntaje
            if (_data.Score.HasValue)
            {
                var scoreColor = _data.Score >= 80 ? "#2E7D32" : _data.Score >= 60 ? "#E65100" : "#C62828";
                var resultColor = _data.Passed == true ? "#2E7D32" : "#C62828";
                var resultBg    = _data.Passed == true ? "#E8F5E9"  : "#FFEBEE";
                var resultText  = _data.Passed == true ? "APROBADO" : "REPROBADO";

                col.Item().PaddingTop(8).Row(r =>
                {
                    r.RelativeItem().Background(scoreColor).Padding(10).Column(s =>
                    {
                        s.Item().Text("PUNTAJE FINAL").Bold().FontColor(Colors.White).FontSize(10);
                        s.Item().Text($"{_data.Score.Value:F1}%").Bold().FontColor(Colors.White).FontSize(26);
                    });
                    r.ConstantItem(8);
                    r.RelativeItem().Background(resultBg).Padding(10).AlignMiddle()
                        .Text(resultText).Bold().FontSize(20).FontColor(resultColor);
                });
            }

            // Notas generales
            if (!string.IsNullOrWhiteSpace(_data.Notes))
            {
                col.Item().PaddingTop(6).Column(n =>
                {
                    n.Item().Text("Observaciones Generales").Bold().FontSize(10);
                    n.Item().Background("#FFFDE7").Padding(6).Text(_data.Notes).FontSize(8).Italic();
                });
            }

            // Secciones
            col.Item().PaddingTop(10).Text("Detalle de Respuestas").Bold().FontSize(11);

            foreach (var section in _data.Sections)
            {
                col.Item().PaddingTop(6).Column(sec =>
                {
                    sec.Item().Background("#1565C0").Padding(5)
                        .Text($"{section.OrderIndex}. {section.SectionName}")
                        .Bold().FontColor(Colors.White).FontSize(9);

                    sec.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.ConstantColumn(55);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(16);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#E3F2FD").Padding(3).Text("Pregunta").Bold().FontSize(8);
                            h.Cell().Background("#E3F2FD").Padding(3).Text("Respuesta").Bold().FontSize(8);
                            h.Cell().Background("#E3F2FD").Padding(3).Text("Notas").Bold().FontSize(8);
                            h.Cell().Background("#E3F2FD").Padding(3).Text("R").Bold().FontSize(8);
                        });

                        foreach (var ans in section.Answers)
                        {
                            var bg = ans.IsNa          ? "#F5F5F5"
                                   : ans.IsConforming == true  ? "#E8F5E9"
                                   : ans.IsConforming == false ? "#FFEBEE"
                                   : Colors.White;

                            var resultSymbol = ans.IsNa            ? "N/A"
                                             : ans.IsConforming == true  ? "✓"
                                             : ans.IsConforming == false ? "✗"
                                             : "—";
                            var symColor = ans.IsConforming == true ? "#2E7D32"
                                         : ans.IsConforming == false ? "#C62828"
                                         : "#888888";

                            table.Cell().Background(bg).Padding(3).Column(qcol =>
                            {
                                qcol.Item().Text(text =>
                                {
                                    text.Span(ans.QuestionText).FontSize(8);
                                    if (ans.IsCritical)
                                        text.Span(" ★").Bold().FontColor("#C62828").FontSize(7);
                                });
                            });
                            table.Cell().Background(bg).Padding(3)
                                .Text(ans.AnswerValue ?? "-").FontSize(8);
                            table.Cell().Background(bg).Padding(3)
                                .Text(ans.Notes ?? "").FontSize(7).Italic();
                            table.Cell().Background(bg).Padding(3).AlignCenter()
                                .Text(resultSymbol).FontSize(9).FontColor(symColor).Bold();
                        }
                    });
                });
            }
        });
    }

    // ── Footer ───────────────────────────────────────────────────────────────
    private void ComposeFooter(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor("#CCCCCC");
            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem()
                    .Text("ITO Cloud — Plataforma de Inspección Técnica de Obras")
                    .FontSize(7).FontColor("#888888").Italic();
                row.ConstantItem(80).AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7).FontColor("#888888");
                    text.CurrentPageNumber().FontSize(7).FontColor("#888888");
                    text.Span(" de ").FontSize(7).FontColor("#888888");
                    text.TotalPages().FontSize(7).FontColor("#888888");
                });
            });
        });
    }

    // ── Helper ───────────────────────────────────────────────────────────────
    private static void AddLabelValue(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.ConstantItem(75).Text(label + ":").Bold().FontSize(8).FontColor("#555555");
            r.RelativeItem().Text(value).FontSize(8);
        });
    }
}
