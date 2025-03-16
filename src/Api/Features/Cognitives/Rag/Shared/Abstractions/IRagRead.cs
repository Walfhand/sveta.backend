using Api.Features.Projects.Domain;

namespace Api.Features.Cognitives.Rag.Shared.Abstractions;

public interface IRagRead
{
    Task<List<(string key, string value, string link)>> ReadAsync(ProjectId projectId,
        string question,
        CancellationToken ct);
}