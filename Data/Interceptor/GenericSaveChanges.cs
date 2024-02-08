using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace notion_clone.Data.Interceptor;

public class GenericSaveChanges : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }


        var now = DateTime.Now;

        var entries = eventData.Context.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // var idProperty = entry.Metadata.FindPrimaryKey().Properties.FirstOrDefault();
                    var idProperty = entry.Metadata.FindProperty("Id");
                    if (idProperty == null)
                    {
                        continue;
                    }

                    if (idProperty.ClrType == typeof(int) && idProperty.Name == "Id")
                    {
                        entry.Property(idProperty.Name).CurrentValue = Guid.NewGuid();
                    }


                    var createdAtProperty = entry.Metadata.FindProperty("CreatedAt");
                    if (createdAtProperty == null)
                    {
                        continue;
                    }

                    if (createdAtProperty.ClrType == typeof(DateTime))
                    {
                        entry.Property(createdAtProperty.Name).CurrentValue = now;
                    }


                    var updatedAtProperty = entry.Metadata.FindProperty("UpdatedAt");
                    if (updatedAtProperty == null)
                    {
                        continue;
                    }

                    if (updatedAtProperty.ClrType == typeof(DateTime))
                    {
                        entry.Property(updatedAtProperty.Name).CurrentValue = null;
                    }


                    var updateUserProperty = entry.Metadata.FindProperty("UpdateUser");
                    if (updateUserProperty == null)
                    {
                        continue;
                    }

                    if (updateUserProperty.ClrType == typeof(Guid))
                    {
                        entry.Property(updateUserProperty.Name).CurrentValue = null;
                    }

                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                case EntityState.Modified:
                default:
                    continue;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}