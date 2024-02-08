using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using notion_clone.Data.Entity;
using notion_clone.Data.Entity.Model;

namespace notion_clone.Data;

public class NotionCloneDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
    IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public DbSet<CategoryEntity> CategoryEntities { get; set; }
    
    public DbSet<PostEntity> PostEntities { get; set; }
    
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    
    public NotionCloneDbContext(DbContextOptions<NotionCloneDbContext> context) : base(context)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        //true yaparsan include yapmana gerek kalmaz ama her seyi baglamaya calisir o yuzden false :)
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}