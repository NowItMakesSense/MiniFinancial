using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Balance)
                .HasPrecision(18, 2);

            builder.Property(x => x.AllowNegativeBalance)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasMany(x => x.Transactions)
                   .WithOne()
                   .HasForeignKey(x => x.OriginAccountId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithOne()
                   .HasForeignKey<Account>(a => a.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();
        }
    }
}
