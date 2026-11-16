using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.Users;

namespace TeamHub.Infrastructure.Persistence.Configurations;

public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // FCM token dài hơn Expo token (thường 150-200 ký tự, có thể hơn) nên chừa dư; 512 byte x2 vẫn dưới giới hạn khóa index 1700 byte
        builder.Property(x => x.Token).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Platform).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
