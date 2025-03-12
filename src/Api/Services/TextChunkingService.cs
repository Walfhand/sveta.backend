namespace Api.Services;

public class TextChunkingService
{
    private readonly int _maxTokensPerChunk;
    private readonly int _overlapTokens;

    public TextChunkingService(int maxTokensPerChunk = 300, int overlapTokens = 50)
    {
        _maxTokensPerChunk = maxTokensPerChunk;
        _overlapTokens = overlapTokens;
    }

    public List<string> ChunkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Estimation approximative : 1 token ≈ 4 caractères pour l'anglais/français
        // Pour être prudent, on utilise 3.5 caractères par token
        var maxCharsPerChunk = (int)(_maxTokensPerChunk * 3.5);
        var overlapChars = (int)(_overlapTokens * 3.5);

        var chunks = new List<string>();

        // Si le texte est déjà assez petit, le retourner tel quel
        if (text.Length <= maxCharsPerChunk)
        {
            chunks.Add(text);
            return chunks;
        }

        // Diviser le texte en chunks avec chevauchement
        var startIndex = 0;
        while (startIndex < text.Length)
        {
            var length = Math.Min(maxCharsPerChunk, text.Length - startIndex);

            // Essayer de terminer le chunk à la fin d'une phrase ou d'un paragraphe
            if (startIndex + length < text.Length)
            {
                var endIndex = startIndex + length;

                // Chercher la fin d'une phrase ou d'un paragraphe
                var sentenceEnd = text.LastIndexOfAny(new[] { '.', '!', '?', '\n' }, endIndex,
                    Math.Min(50, endIndex - startIndex));

                if (sentenceEnd > startIndex && sentenceEnd < endIndex) length = sentenceEnd - startIndex + 1;
            }

            chunks.Add(text.Substring(startIndex, length));

            // Avancer avec chevauchement
            startIndex += length - overlapChars;
            if (startIndex < 0) startIndex = 0; // Sécurité

            // Éviter les boucles infinies
            if (length <= overlapChars) startIndex = text.Length;
        }

        return chunks;
    }
}