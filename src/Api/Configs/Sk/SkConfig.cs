using Api.Configs.Sk.Options;
using Api.Features.Projects.Features.Documents.UploadDocuments.Services;
using Microsoft.SemanticKernel;

namespace Api.Configs.Sk;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0050
public static class SkConfig
{
    public static IServiceCollection AddSk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SkOptions>(configuration.GetSection(SkOptions.Sk));
        var skOptions = configuration.GetSection(SkOptions.Sk).Get<SkOptions>();

        services.AddOpenAIChatCompletion("meta-llama/Llama-3.2-90B-Vision-Instruct",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey);

        services.AddRedisVectorStore(skOptions.StoreUri);
        services.AddHuggingFaceTextEmbeddingGeneration(new Uri(skOptions.EmbeddingUri));

        services.AddSingleton<KernelPluginCollection>(serviceProvider =>
            [
            ]
        );

        services.AddTransient(serviceProvider =>
        {
            var pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
            return new Kernel(serviceProvider, pluginCollection);
        });

        services.AddScoped<DocumentUploader>();
        services.AddHttpContextAccessor();
        return services;
    }
}