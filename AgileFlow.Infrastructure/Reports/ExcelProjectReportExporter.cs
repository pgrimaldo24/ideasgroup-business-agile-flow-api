using ClosedXML.Excel;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;

namespace AgileFlow.Infrastructure.Reports;

public class ExcelProjectReportExporter : IProjectReportExporter
{
    public string Format => "xlsx";
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileExtension => "xlsx";

    public byte[] Export(ProjectReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Reporte");

        sheet.Cell(1, 1).Value = report.ProjectName;
        sheet.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);

        sheet.Cell(2, 1).Value = report.ProjectDescription ?? string.Empty;
        sheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        sheet.Cell(3, 1).Value = $"Fecha de generación: {report.GeneratedAtUtc:dd/MM/yyyy HH:mm} UTC";
        sheet.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

        const int headerRow = 5;
        var headers = new[] { "Tarea", "Columna", "Responsable", "Prioridad" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#1F5FA8"));
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = headerRow + 1;
        foreach (var task in report.Tasks)
        {
            sheet.Cell(row, 1).Value = task.TaskTitle;
            sheet.Cell(row, 2).Value = task.ColumnName;
            sheet.Cell(row, 3).Value = task.AssigneeName ?? "-";
            sheet.Cell(row, 4).Value = task.Priority;
            row++;
        }

        // Anchos de columna adecuados (req. 6.8), en vez de dejar el ancho
        // por defecto de Excel, que suele truncar títulos largos.
        sheet.Column(1).Width = 40;
        sheet.Column(2).Width = 20;
        sheet.Column(3).Width = 25;
        sheet.Column(4).Width = 15;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
