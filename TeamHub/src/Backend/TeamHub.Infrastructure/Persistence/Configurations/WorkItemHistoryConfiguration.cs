using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Infrastructure.Persistence.Configurations;

public sealed class WorkItemHistoryConfiguration : IEntityTypeConfiguration<WorkItemHistory>
{
    public void Configure(EntityTypeBuilder<WorkItemHistory> builder)
    {
        builder.ToTable("WorkItemHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Field).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.OldValue).HasMaxLength(300);
        builder.Property(x => x.NewValue).HasMaxLength(300);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChangedBy)
            .WithMany()
            .HasForeignKey(x => x.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WorkItemId, x.ChangedAt });
        builder.HasIndex(x => x.TeamId);
    }
}
