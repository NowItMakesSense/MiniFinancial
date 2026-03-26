using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
