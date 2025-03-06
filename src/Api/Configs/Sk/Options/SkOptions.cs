namespace Api.Configs.Sk.Options;

public class SkOptions
{
    public const string Sk = "Sk";
    public string ApiKey { get; set; } = null!;
    public string Uri { get; set; } = null!;
    public string EmbeddingUri { get; set; } = null!;
    public string StoreUri { get; set; } = null!;
}