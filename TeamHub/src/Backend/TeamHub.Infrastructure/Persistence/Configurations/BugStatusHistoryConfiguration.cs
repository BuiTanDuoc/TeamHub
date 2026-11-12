using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Teams;

namespace TeamHub.Infrastructure.Persistence.Configurations;

public sealed class BugStatusHistoryConfiguration : IEntityTypeConfiguration<BugStatusHistory>
{
    public void Configure(EntityTypeBuilder<BugStatusHistory> builder)
    {
        builder.ToTable("BugStatusHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(2000);

        builder.HasOne(x => x.ChangedBy)
            .WithMany()
            .HasForeignKey(x => x.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BugId, x.ChangedAt });
        builder.HasIndex(x => x.TeamId);
    }
}
