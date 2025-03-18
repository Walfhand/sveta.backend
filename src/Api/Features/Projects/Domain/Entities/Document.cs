using System.Security.Cryptography;
using Engine.Core.Models;

namespace Api.Features.Projects.Domain.Entities;

public record DocumentId : IdBase
{
    public DocumentId()
    {
    }

    public DocumentId(Guid id) : base(id)
    {
    }
}

public enum DocumentType
{
    Code,
    Business,
    TechnicalAnalysis,
    Documentation
}

public class Document : Entity<DocumentId>
{
    private Document()
    {
    }

    private Document(string name, string contentHash, string contentType)
    {
        Name = name;
        ContentHash = contentHash;
        ContentType = contentType;
    }

    public string Name { get; private set; } = null!;
    public string ContentHash { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;


    public static Document Create(string name, byte[] content,
        string contentType)
    {
        return new Document(name, CreateSha256Hash(content), contentType);

        static string CreateSha256Hash(byte[] data)
        {
            var byteHash = SHA256.HashData(data);
            return Convert.ToHexStringLower(byteHash);
        }
    }
}