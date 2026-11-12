using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Infrastructure.Persistence.Configurations;

public sealed class BugConfiguration : IEntityTypeConfiguration<Bug>
{
    public void Configure(EntityTypeBuilder<Bug> builder)
    {
        builder.ToTable("Bugs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Environment).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.AffectedVersion).HasMaxLength(50);
        builder.Property(x => x.FixedInVersion).HasMaxLength(50);
        builder.Property(x => x.DeviceInfo).HasMaxLength(500);
        builder.Property(x => x.StepsToReproduce).IsRequired();   // nvarchar(max)
        builder.Property(x => x.ExpectedResult).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ActualResult).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.ProjectId, x.Number }).IsUnique();
        builder.HasIndex(x => new { x.TeamId, x.ProjectId, x.Status });
        builder.HasIndex(x => new { x.AssigneeId, x.Status });
        builder.HasIndex(x => new { x.Severity, x.Status });
        builder.HasIndex(x => x.SlaDueAt); // BugSlaJob quét bug sắp/đã quá hạn
        builder.HasIndex(x => x.LinkedWorkItemId);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reporter)
            .WithMany()
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assignee)
            .WithMany()
            .HasForeignKey(x => x.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LinkedWorkItem)
            .WithMany()
            .HasForeignKey(x => x.LinkedWorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DuplicateOf)
            .WithMany()
            .HasForeignKey(x => x.DuplicateOfBugId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(x => x.BugId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.History).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
