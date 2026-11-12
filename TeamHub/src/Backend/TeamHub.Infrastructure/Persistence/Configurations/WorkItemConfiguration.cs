using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Infrastructure.Persistence.Configurations;

public sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("WorkItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description); // nvarchar(max)
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.EstimateHours).HasPrecision(7, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();

        // Số thứ tự không đổi và không dùng lại, kể cả khi item bị xóa mềm
        builder.HasIndex(x => new { x.ProjectId, x.Number }).IsUnique();
        builder.HasIndex(x => new { x.TeamId, x.ProjectId, x.Status });
        builder.HasIndex(x => new { x.AssigneeId, x.Status });
        builder.HasIndex(x => x.SprintId);
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.DueDate);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sprint)
            .WithMany()
            .HasForeignKey(x => x.SprintId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reporter)
            .WithMany()
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assignee)
            .WithMany()
            .HasForeignKey(x => x.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reviewer)
            .WithMany()
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
