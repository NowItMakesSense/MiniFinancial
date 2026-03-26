using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniFinancial.Domain.Entities;

namespace MiniFinancial.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();
        }
    }
}
