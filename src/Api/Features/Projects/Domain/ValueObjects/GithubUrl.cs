using System.Text.RegularExpressions;

namespace Api.Features.Projects.Domain.ValueObjects;

public partial record GithubUrl
{
    private GithubUrl()
    {
    }

    private GithubUrl(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = null!;

    public static GithubUrl Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("GitHub URL cannot be empty", nameof(url));

        var regex = GithubUrlRegex();
        if (!regex.IsMatch(url))
            throw new ArgumentException("Invalid GitHub repository URL format", nameof(url));

        return new GithubUrl(url);
    }

    public string GetOwner()
    {
        var parts = Value.TrimEnd('/').Split('/');
        return parts[^2];
    }

    public string GetRepository()
    {
        var parts = Value.TrimEnd('/').Split('/');
        return parts[^1];
    }

    [GeneratedRegex(@"^https://github\.com/[\w-]+/[\w.-]+/?$")]
    private static partial Regex GithubUrlRegex();
}