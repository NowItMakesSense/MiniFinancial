using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(t => t.CategoryUserName)
                   .HasMaxLength(150);

            builder.Property(x => x.Type)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.Category)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.IsRecurring)
                .IsRequired();

            builder.Property(x => x.IsReversal);

            builder.Property(x => x.ReversedTransactionId);

            builder.Property(x => x.Version)
                   .IsConcurrencyToken();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasIndex(x => x.OriginAccountId);
            builder.HasIndex(x => x.DestinyAccountId);
            builder.HasIndex(x => x.CategoryUserId);
            builder.HasIndex(x => x.OccurredAt);

            builder.HasOne<Category>()
                   .WithMany()
                   .HasForeignKey(x => x.CategoryUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
