using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Conversations.Ask.Endpoints;

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
    : PostMinimalEndpoint<AskRequest, AskResponse>("conversations/{id:guid}/ask")
{
    private const string DefaultCollection = "api_knowledge_base";
    protected override Delegate Handler => Endpoint;

    private static async Task<IResult> Endpoint([AsParameters] AskRequest request, Kernel kernel)
    {
        var collection = string.IsNullOrEmpty(request.Body.Collection) ? DefaultCollection : request.Body.Collection;

        var promptTemplate = @"
Répond à cette question en Français: {{$input}}
Basé sur ces informations: {{TextMemory.Recall $input collection=$collection limit=$limit relevance=$relevance}}
Si les informations ne contiennent pas les informations pour y répondre, alors tu répondra en disant 'Je ne sais pas'
";

        var response = await kernel.InvokePromptAsync(
            promptTemplate,
            new KernelArguments
            {
                { "input", request.Body.Question },
                { "limit", 3 },
                { "relevance", 0.7 },
                { "collection", collection }
            }
        );

        return Results.Ok(new { answer = response.ToString() });
    }
}