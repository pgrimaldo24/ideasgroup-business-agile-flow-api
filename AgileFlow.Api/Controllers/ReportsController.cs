using AgileFlow.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileFlow.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly GenerateProjectReportUseCase _generateReportUseCase;

    public ReportsController(GenerateProjectReportUseCase generateReportUseCase) =>
        _generateReportUseCase = generateReportUseCase;

    /// <summary>
    /// Descarga el reporte del proyecto en el formato pedido ("pdf" o "xlsx").
    /// Un tercer formato futuro solo necesita registrar un nuevo
    /// IProjectReportExporter — esta ruta no cambia (req. 6.8).
    /// </summary>
    [HttpGet("{format}")]
    public async Task<IActionResult> Download(Guid projectId, string format, CancellationToken ct)
    {
        var file = await _generateReportUseCase.ExecuteAsync(projectId, format, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
