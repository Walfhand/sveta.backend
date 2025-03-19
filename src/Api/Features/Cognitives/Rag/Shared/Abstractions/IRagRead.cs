using Api.Features.Cognitives.Rag.Search;
using Api.Features.Projects.Domain;

namespace Api.Features.Cognitives.Rag.Shared.Abstractions;

public interface IRagRead
{
    Task<List<ChunkResult>> ReadAsync(ProjectId projectId,
        string question,
        CancellationToken ct,
        VectorSearchOptions? options = null);
}