namespace notion_clone.Data.Entity.Model;

public class CategoryEntity : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}