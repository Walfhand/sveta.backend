using Api.Features.Chats.CreateConversation.Results;
using Api.Features.Projects.Domain;
using Engine.Exceptions;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Chats.CreateConversation.Endpoints;

public record CreateConversationRequest
{
    [FromRoute] public Guid ProjectId { get; set; }
}

public class CreateConversationEndpoint()
    : PostMinimalEndpoint<CreateConversationRequest, ConversationResult>("projects/{projectId:guid}/conversations");

public class CreateConversationRequestHandler(
    IAppDbContextFactory dbContextFactory,
    IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<ConversationResult> Handle(CreateConversationRequest request, CancellationToken ct)
    {
        var project = await DbContext.Set<Project>()
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);
        if (project is null)
            throw project.NotFound(new ProjectId(request.ProjectId));

        return project.StartNewConversation().ToResult();
    }
}