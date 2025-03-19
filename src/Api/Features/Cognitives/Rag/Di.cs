using Api.Features.Cognitives.Rag.Search;
using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Api.Features.Cognitives.Rag.Shared.Implementations;

namespace Api.Features.Cognitives.Rag;

public static class Di
{
    public static IServiceCollection AddCognitiveServices(this IServiceCollection services)
    {
        services.AddSingleton<IRagRead, RagService>();
        services.AddSingleton<IRagWrite, RagService>();

        services.AddSingleton<VectorSearchService>();
        return services;
    }
}