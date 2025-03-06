using Api.Configs.Sk.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

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
        
        services.AddOpenAIChatCompletion(modelId: "ozone-ai/0x-lite",
            endpoint: new Uri(skOptions!.Uri),
            apiKey: skOptions.ApiKey);

        services.AddHuggingFaceTextEmbeddingGeneration(endpoint: new Uri(skOptions.EmbeddingUri));
        
        services.AddSingleton<IMemoryStore>(sp => 
            new RedisMemoryStore(skOptions.StoreUri, vectorSize: 1024)
        );

        services.AddSingleton<ISemanticTextMemory>(sp =>
        {
            var memoryStore = sp.GetRequiredService<IMemoryStore>();
            var embeddingService = sp.GetRequiredService<ITextEmbeddingGenerationService>();
    
            return new SemanticTextMemory(memoryStore, embeddingService);
        });
        
        services.AddSingleton(sp =>
        {
            var memoryStore = sp.GetRequiredService<IMemoryStore>();
            
            var embeddingService = sp.GetRequiredService<ITextEmbeddingGenerationService>();
            var semanticTextMemory = new SemanticTextMemory(memoryStore, embeddingService);
            
            return new TextMemoryPlugin(semanticTextMemory);
        });
        
        services.AddSingleton<KernelPluginCollection>((serviceProvider) => 
            [
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<TextMemoryPlugin>(), "TextMemory"),
            ]
        );
        
        services.AddTransient((serviceProvider)=> {
            var pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
            return new Kernel(serviceProvider, pluginCollection);
        });
        return services;
    }
}