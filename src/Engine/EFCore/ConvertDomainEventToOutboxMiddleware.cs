using Engine.Core.Events;
using Engine.Core.Models;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;

namespace Engine.EFCore;

public class ConvertDomainEventToOutboxMiddleware
{
    public async Task After(IAppDbContextFactory dbContextFactory, IDbContextOutbox outbox, IMessageBus messageBus,
        IEventMapper eventMapper,
        IHttpContextAccessor contextAccessor)
    {
        if (contextAccessor.HttpContext!.Request.Method == "GET")
            return;

        if (dbContextFactory.CreateDbContext() is AppDbContextBase dbContext)
        {
            outbox.Enroll(dbContext);

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                while (true)
                {
                    var entities = dbContext.ChangeTracker.Entries<IAggregateRoot>()
                        .Select(x => x.Entity)
                        .ToList();
                    var events =
                        entities.SelectMany(x => x.ClearDomainEvents())
                            .ToList();
                    if (events.Count != 0)
                    {
                        foreach (var @event in events)
                        {
                            if (@event is not IDomainEvent domainEvent) continue;

                            var integrationEvent = eventMapper.MapToIntegrationEvent(domainEvent);

                            if (integrationEvent is not null)
                                await outbox.PublishAsync(integrationEvent);
                            try
                            {
                                await messageBus.InvokeAsync(domainEvent);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        continue;
                    }

                    break;
                }

                // Sauvegarde des modifications et publication des messages
                await outbox.SaveChangesAndFlushMessagesAsync();
            });
        }
    }
}