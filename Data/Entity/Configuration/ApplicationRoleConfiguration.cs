using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace notion_clone.Data.Entity.Configuration
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}