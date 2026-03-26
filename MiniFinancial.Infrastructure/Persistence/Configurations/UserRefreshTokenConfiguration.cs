using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("UserRefreshToken");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.HasOne<User>()
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId);
        }
    }
}
