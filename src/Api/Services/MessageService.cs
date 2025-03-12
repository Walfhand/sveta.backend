using Wolverine;
using IMessage = QuickApi.Abstractions.Cqrs.IMessage;

namespace Api.Services;

public class MessageService(IMessageBus messageBus) : IMessage
{
    public async Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct)
    {
        return await messageBus.InvokeAsync<TResult>(message, ct);
    }

    public async Task InvokeAsync(object message, CancellationToken ct)
    {
        await messageBus.InvokeAsync(message, ct);
    }
}