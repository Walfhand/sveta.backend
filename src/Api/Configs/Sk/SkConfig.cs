using Api.Configs.Sk.Options;
using Api.Features.Chats.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Postgres;

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

        services.AddOpenAIChatCompletion("deepseek-ai/DeepSeek-R1-Distill-Qwen-32B",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "business");

        services.AddOpenAIChatCompletion("deepseek-ai/DeepSeek-R1",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "code");

        services.AddOpenAIChatCompletion("ozone-ai/0x-lite",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "decision");

        services.AddOpenAIChatCompletion("microsoft/phi-4",
            new Uri(skOptions!.Uri),
            skOptions.ApiKey,
            serviceId: "classification");

        //services.AddRedisVectorStore(skOptions.StoreUri);
        services.AddPostgresVectorStore(skOptions.StoreUri, new PostgresVectorStoreOptions { Schema = "rag" });
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
        services.AddSingleton<ClassificationAgent>();
        services.AddHttpContextAccessor();
        return services;
    }
}