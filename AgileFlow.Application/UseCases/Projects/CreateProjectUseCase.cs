using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Enums;

namespace AgileFlow.Application.UseCases.Projects;

public record CreateProjectCommand(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    string Status);

public class CreateProjectUseCase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectUseCase(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> ExecuteAsync(CreateProjectCommand command, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ProjectStatus>(command.Status, ignoreCase: true, out var status))
        {
            throw new AppException(
                $"Estado de proyecto no válido: '{command.Status}'. Valores admitidos: " +
                string.Join(", ", Enum.GetNames<ProjectStatus>()) + ".");
        }

        var project = new Project(
            command.Name, command.Description, command.StartDate, command.ExpectedEndDate, status);

        await _projectRepository.AddAsync(project, ct);
        await _unitOfWork.CommitAsync(ct);

        return ToDto(project);
    }

    private static ProjectDto ToDto(Project project) => new(
        project.Id, project.Name, project.Description, project.StartDate,
        project.ExpectedEndDate, project.Status.ToString());
}
