using UglyToad.PdfPig;

namespace Api.Shared.Files;

public class PdfContentExtractor
{
    public PdfContent ExtractContent(byte[] pdfBytes)
    {
        var result = new PdfContent();

        using var document = PdfDocument.Open(pdfBytes);

        // Extraire le texte page par page
        foreach (var page in document.GetPages())
        {
            var pageText = page.Text;
            result.Pages.Add(pageText);

            // Extraire les images de la page
            foreach (var image in page.GetImages())
            {
                var imageContent = image.RawBytes.ToArray();
                result.Images.Add(imageContent);
            }
        }

        // Concaténer tout le texte
        result.Text = string.Join(" ", result.Pages);

        return result;
    }

    public record PdfContent
    {
        public string Text { get; set; } = string.Empty;
        public List<byte[]> Images { get; set; } = [];
        public List<string> Pages { get; set; } = [];
    }
}