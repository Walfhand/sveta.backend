using Api.Configs.Cqrs;
using Api.Shared.Files;
using Api.Shared.Files.Extractors.Implementations;
using Api.Shared.Integrations;
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
        services.AddSingleton<PdfFileContentExtractor>();
        services.AddFileExtractors();
        services.AddCustomProblemDetails();
        return services;
    }
}