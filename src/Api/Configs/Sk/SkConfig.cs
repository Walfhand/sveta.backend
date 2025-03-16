using Api.Configs.Sk.Options;
using Api.Features.Projects.Features.Conversations.Agents;
using Api.Shared.Rag.Abstractions;
using Api.Shared.Rag.Implementations;
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

        services.AddOpenAIChatCompletion("microsoft/phi-4",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "business");

        services.AddOpenAIChatCompletion("Qwen/Qwen2.5-Coder-32B-Instruct",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "code");

        services.AddOpenAIChatCompletion("ozone-ai/0x-lite",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "decision");

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

        services.AddSingleton<BusinessAgent>();
        services.AddSingleton<OnboardingAgent>();
        services.AddSingleton<CodeAgent>();
        services.AddSingleton<DecisionAgent>();

        services.AddSingleton<IRagRead, RagService>();
        services.AddSingleton<IRagWrite, RagService>();
        services.AddHttpContextAccessor();
        return services;
    }
}