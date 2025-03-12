using Api.Features.Projects.Domain;

namespace Api.Features.Projects.Features.CreateProject.Results;

public record ProjectResult(Guid Id, string Name, string Description);

public static class ProjectExtensions
{
    public static ProjectResult ToResult(this Project project)
    {
        return new ProjectResult(project.Id, project.Name, project.Description);
    }
}