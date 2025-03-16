namespace Api.Shared.Files.Extractors.Abstractions;

public interface IFileContentExtractor
{
    string Extract(byte[] content);
}