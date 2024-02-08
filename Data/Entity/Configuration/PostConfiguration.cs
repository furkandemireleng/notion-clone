using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using notion_clone.Data.Entity.Model;


namespace notion_clone.Data.Entity.Configuration
{
    public class PostConfiguration : IEntityTypeConfiguration<PostEntity>
    {
        public void Configure(EntityTypeBuilder<PostEntity> builder)
        {
            builder.ToTable("Post");

            // Add configuration for integer primary key
            builder.Property(t => t.Id)
                .IsRequired()
                .HasColumnType("uuid")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(255);


            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder
                .HasMany(p => p.Categories)
                .WithMany(); // No need for navigation property on Category side
        }
    }
}