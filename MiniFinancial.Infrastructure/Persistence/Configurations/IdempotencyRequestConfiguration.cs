using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class IdempotencyRequestConfiguration : IEntityTypeConfiguration<IdempotencyRequest>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRequest> builder)
        {
            builder.ToTable("IdempotencyRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.RequestHash)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ResponseJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken();

            builder.HasIndex(x => new { x.UserId, x.Key })
                .IsUnique();
        }
    }
}
