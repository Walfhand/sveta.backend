using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Features.Conversations.Agents;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Conversations.Ask.Endpoints;
#pragma warning disable SKEXP0001
public record AskRequest
{
    [FromRoute] public Guid ProjectId { get; set; }
    [FromRoute] public Guid ConversationId { get; set; }
    [FromBody] public AskRequestBody Body { get; set; } = null!;

    public record AskRequestBody
    {
        public string Question { get; set; } = null!;
    }
}

public record AskResponse
{
    public string Message { get; set; } = null!;
}

public class AskEndpoint()
    : PostMinimalEndpoint<AskRequest, AskResponse>("projects/{projectId}/conversations/{conversationId:guid}/ask");

public class AskRequestHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<AskResponse> Handle(AskRequest request, DecisionAgent decisionAgent,
        CancellationToken ct)
    {
        var project = await DbContext.Set<Project>()
            .Include(x => x.Conversations.Where(c => c.Id == request.ConversationId))
            .ThenInclude(m => m.ChatMessages)
            .AsSplitQuery()
            .FirstAsync(x => x.Id == request.ProjectId, ct);

        var conversation = project.GetConversation(new ConversationId(request.ConversationId));
        var response =
            await decisionAgent.RouteAndAnswerAsync(request.Body.Question, project.Id, project.Name, conversation, ct);

        conversation.AddChatMessage("user", request.Body.Question);
        conversation.AddChatMessage("assistant", response);
        return new AskResponse
        {
            Message = response
        };
    }
}