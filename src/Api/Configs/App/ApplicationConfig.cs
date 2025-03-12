using Api.Integrations;
using Api.Services;
using Engine.Core.Events;
using Engine.ProblemDetails;
using QuickApi.Abstractions.Cqrs;

namespace Api.Configs.App;

public static class ApplicationConfig
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMessage, MessageService>();
        services.AddScoped<IEventMapper, EventMapper>();
        services.AddSingleton<PdfContentExtractor>();
        services.AddSingleton<TextChunkingService>(sp =>
            new TextChunkingService());
        services.AddCustomProblemDetails();
        return services;
    }
}