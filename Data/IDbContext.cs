namespace notion_clone.Data;

public interface IDbContext
{
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}