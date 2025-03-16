using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;

namespace Api.Features.Chats.Agents;

public interface IAgent
{
    public string Description { get; }

    Task<string> AnswerAsync(ProjectId projectId, string projectName, string question,
        Conversation conversation,
        CancellationToken ct);
}