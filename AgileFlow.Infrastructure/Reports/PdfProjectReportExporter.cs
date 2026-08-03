using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgileFlow.Infrastructure.Reports;

public class PdfProjectReportExporter : IProjectReportExporter
{
    public string Format => "pdf";
    public string ContentType => "application/pdf";
    public string FileExtension => "pdf";

    public byte[] Export(ProjectReportDto report)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(report.ProjectName).FontSize(18).Bold();

                    if (!string.IsNullOrWhiteSpace(report.ProjectDescription))
                        column.Item().Text(report.ProjectDescription).FontSize(10).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(4).Text(
                        $"Fecha de generación: {report.GeneratedAtUtc:dd/MM/yyyy HH:mm} UTC")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Tarea
                        columns.RelativeColumn(2); // Columna
                        columns.RelativeColumn(2); // Responsable
                        columns.RelativeColumn(1); // Prioridad
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Tarea");
                        header.Cell().Element(HeaderCell).Text("Columna");
                        header.Cell().Element(HeaderCell).Text("Responsable");
                        header.Cell().Element(HeaderCell).Text("Prioridad");

                        static IContainer HeaderCell(IContainer c) => c
                            .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                            .Background(Colors.Blue.Darken2)
                            .Padding(5);
                    });

                    foreach (var row in report.Tasks)
                    {
                        table.Cell().Element(BodyCell).Text(row.TaskTitle);
                        table.Cell().Element(BodyCell).Text(row.ColumnName);
                        table.Cell().Element(BodyCell).Text(row.AssigneeName ?? "-");
                        table.Cell().Element(BodyCell).Text(row.Priority);
                    }

                    static IContainer BodyCell(IContainer c) => c
                        .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
