using Dapper;
using Engine.EFCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Api.Features.Cognitives.Rag.Search;

public class ChunkResult
{
    public string Key { get; init; } = null!;
    public string Text { get; init; } = null!;
    public string DocumentName { get; init; } = null!;
    public int ChunkNumber { get; init; }
    public string Category1 { get; init; } = null!;
    public string Category2 { get; init; } = null!;
    public float ScoreCategory1 { get; init; }
    public float ScoreCategory2 { get; init; }
    public float TotalScore { get; init; }
}

public record VectorSearchOptions(
    float VectorWeight = 0.7f,
    float Category1Weight = 0.2f,
    float Category2Weight = 0.1f,
    int MaxResults = 10,
    string? Category1Filter = null,
    string? Category2Filter = null);

public class VectorSearchService(IOptions<Postgres> postgresOptions)
{
    private readonly Postgres _postgres = postgresOptions.Value;

    public async Task<List<ChunkResult>> HybridVectorSearch(
        float[] inputVector,
        string collectionName,
        VectorSearchOptions? options = null)
    {
        var newOptions = options ?? new VectorSearchOptions();
        var sql = $"""
                   SELECT 
                       "Key",
                       "DocumentName",
                       "ChunkNumber",
                       "Text",
                       "Category1",
                       "Category2",
                       "ScoreCategory1",
                       "ScoreCategory2",
                       (1 - ("TextEmbedding" <=> @Vector::vector)) * @VectorWeight +
                       (CASE 
                           WHEN @Category1Filter IS NOT NULL AND ("Category1" = @Category1Filter OR "Category2" = @Category1Filter)
                           THEN 
                               CASE
                                   WHEN "Category1" = @Category1Filter THEN "ScoreCategory1" * @Category1Weight
                                   WHEN "Category2" = @Category1Filter THEN "ScoreCategory2" * @Category1Weight
                                   ELSE 0
                               END
                           ELSE 0 
                       END) +
                       (CASE 
                           WHEN @Category2Filter IS NOT NULL AND ("Category1" = @Category2Filter OR "Category2" = @Category2Filter)
                           THEN 
                               CASE
                                   WHEN "Category1" = @Category2Filter THEN "ScoreCategory1" * @Category2Weight
                                   WHEN "Category2" = @Category2Filter THEN "ScoreCategory2" * @Category2Weight
                                   ELSE 0
                               END
                           ELSE 0 
                       END) AS "TotalScore"
                   FROM rag."{collectionName}"
                   {BuildWhereClause(newOptions.Category1Filter, newOptions.Category2Filter)}
                   ORDER BY "TotalScore" DESC
                   LIMIT @MaxResults;
                   """;

        await using var connection = new NpgsqlConnection(_postgres.ConnexionString);
        var results = (await connection.QueryAsync<ChunkResult>(sql, new
        {
            Vector = inputVector,
            newOptions.VectorWeight,
            newOptions.Category1Weight,
            newOptions.Category2Weight,
            newOptions.MaxResults,
            newOptions.Category1Filter,
            newOptions.Category2Filter,
            TableName = collectionName
        })).ToList();

        return results;
    }

    private string BuildWhereClause(string? category1Filter, string? category2Filter)
    {
        var conditions = new List<string>();

        if (!string.IsNullOrEmpty(category1Filter))
            conditions.Add("""
                           ("Category1" = @Category1Filter OR "Category2" = @Category1Filter)
                           """);

        if (!string.IsNullOrEmpty(category2Filter))
            conditions.Add("""
                           ("Category1" = @Category2Filter OR "Category2" = @Category2Filter)
                           """);

        return conditions.Any() ? $"WHERE {string.Join(" OR ", conditions)}" : "";
    }
}