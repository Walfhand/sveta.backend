using Api.Features.Projects.Domain;
using Api.Features.Projects.Features.CreateProject.Results;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.CreateProject.Endpoints;

public record CreateProjectRequest
{
    [FromBody] public CreateProjectRequestBody Body { get; set; } = null!;

    public record CreateProjectRequestBody
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}

public class CreateProjectEndpoint()
    : PostMinimalEndpoint<CreateProjectRequest, ProjectResult>("projects");

public class CreateProjectEndpointHandler(
    IAppDbContextFactory dbContextFactory,
    IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<ProjectResult> Handle(CreateProjectRequest request, CancellationToken ct)
    {
        var test = DbContext.Set<Project>().AsQueryable();
        var project = Project.Create(request.Body.Name, request.Body.Description);
        await DbContext.Set<Project>().AddAsync(project, ct);
        return project.ToResult();
    }
}