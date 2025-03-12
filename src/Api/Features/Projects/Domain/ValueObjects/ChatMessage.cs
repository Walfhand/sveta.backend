namespace Api.Features.Projects.Domain.ValueObjects;

public record ChatMessage(string Role, string Content, int Order);