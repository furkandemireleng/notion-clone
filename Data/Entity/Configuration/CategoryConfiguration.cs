using notion_clone.Data.Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace notion_clone.Data.Entity.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
    {
        public void Configure(EntityTypeBuilder<CategoryEntity> builder)
        {
            builder.ToTable("Category");
            

            // Add configuration for integer primary key
            builder.Property(t => t.Id)
                .IsRequired()
                .HasColumnType("uuid")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(t => t.Name).IsUnique();

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}