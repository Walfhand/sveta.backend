using Api.Shared.Files.Extractors.Abstractions;
using Api.Shared.Files.Extractors.Implementations;

namespace Api.Shared.Files;

public static class Di
{
    public static IServiceCollection AddFileExtractors(this IServiceCollection services)
    {
        services.AddSingleton<IFileContentExtractor, PdfFileContentExtractor>();
        return services;
    }
}