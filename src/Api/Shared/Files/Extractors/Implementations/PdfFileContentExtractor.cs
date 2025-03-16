using Api.Shared.Files.Extractors.Abstractions;
using UglyToad.PdfPig;

namespace Api.Shared.Files.Extractors.Implementations;

public class PdfFileContentExtractor : IFileContentExtractor
{
    public string Extract(byte[] content)
    {
        var result = new PdfContent();

        using var document = PdfDocument.Open(content);

        foreach (var page in document.GetPages())
        {
            var pageText = page.Text;
            result.Pages.Add(pageText);

            foreach (var image in page.GetImages())
            {
                var imageContent = image.RawBytes.ToArray();
                result.Images.Add(imageContent);
            }
        }

        result.Text = string.Join(" ", result.Pages);

        return result.Text;
    }

    public record PdfContent
    {
        public string Text { get; set; } = string.Empty;
        public List<byte[]> Images { get; set; } = [];
        public List<string> Pages { get; set; } = [];
    }
}