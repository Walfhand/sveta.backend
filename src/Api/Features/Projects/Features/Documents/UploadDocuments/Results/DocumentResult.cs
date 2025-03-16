using Api.Features.Projects.Domain.Entities;

namespace Api.Features.Projects.Features.Documents.UploadDocuments.Results;

public record DocumentResult(DocumentId DocumentId, string Name, string ContentHash, string ContentType);

public static class DocumentExtensions
{
    public static DocumentResult ToResult(this Document document)
    {
        return new DocumentResult(document.Id, document.Name, document.ContentHash, document.ContentType);
    }
}