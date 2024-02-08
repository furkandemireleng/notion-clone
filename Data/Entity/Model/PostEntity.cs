namespace notion_clone.Data.Entity.Model;

public class PostEntity : IEntity
{
    public Guid Id { get; set; }

    public string? Title { get; set; }
    
    public string? Content { get; set; }
    
    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }
    // Other properties

    // Navigation property for categories
    public ICollection<CategoryEntity>? Categories { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}