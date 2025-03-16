using Api.Features.Projects.Domain;

namespace Api.Shared.Rag.Abstractions;

public interface IRagRead
{
    Task<List<(string key, string value, string link)>> ReadAsync(ProjectId projectId,
        string question,
        CancellationToken ct);
}