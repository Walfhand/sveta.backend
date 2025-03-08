using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Chat.Endpoints;


public record AskRequest
{
    [FromBody] public AskRequestBody Body { get; set; } = null!;
    public record AskRequestBody
    {
        public string Question { get; set; } = null!;
        public string Collection { get; set; } = null!;
    }
}

public record AskResponse
{
    
}


public class AskEndpoint()
    : PostMinimalEndpoint<AskRequest, AskResponse>("chat/ask")
{
    protected override Delegate Handler => Endpoint;
    private const string DefaultCollection = "api_knowledge_base";
    private static async Task<IResult> Endpoint([AsParameters] AskRequest request, Kernel kernel)
    {
        string collection = string.IsNullOrEmpty(request.Body.Collection) ? DefaultCollection : request.Body.Collection;
        
        string promptTemplate = @"
Répond à cette question en Français: {{$input}}
Basé sur ces informations: {{TextMemory.Recall $input collection=$collection limit=$limit relevance=$relevance}}
Si les informations ne contiennent pas les informations pour y répondre, alors tu répondra en disant 'Je ne sais pas'
";
        
        var response = await kernel.InvokePromptAsync(
            promptTemplate,
            new KernelArguments()
            {
                { "input", request.Body.Question },
                { "limit", 3 },
                { "relevance", 0.7 },
                { "collection", collection }
            }
        );

        return Results.Ok(new { answer = response.ToString()});
    }
}