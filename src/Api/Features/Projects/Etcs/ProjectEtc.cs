using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Domain.ValueObjects;
using Engine.EFCore;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Features.Projects.Etcs;

public class ProjectEtc : BaseETC<Project, ProjectId>
{
    protected override ProjectId KeyCreator(Guid id) => new(id);

    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);
        builder.OwnsOne(x => x.GithubUrl, gitHubUrlBuilder =>
        {
            gitHubUrlBuilder.Property(gitHubUrl => gitHubUrl.Value)
                .HasColumnName(nameof(GithubUrl).Underscore().ToLower());
        });

        builder.OwnsMany(x => x.Conversations,
            conversationBuilder =>
            {
                conversationBuilder.Property(x => x.Id)
                    .HasConversion(id => id.Value, value => new ConversationId(value))
                    .ValueGeneratedNever();
                conversationBuilder.OwnsMany(x => x.ChatMessages);
            });

        builder.OwnsMany(x => x.Documents, documentBuilder =>
        {
            documentBuilder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new DocumentId(value))
                .ValueGeneratedNever();
        });
    }
}