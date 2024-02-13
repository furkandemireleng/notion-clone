using notion_clone.Data.Entity.Model;

namespace notion_clone.Dto.Category;

public class PostResponseDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }
    
    public string? Content { get; set; }

    public List<CategoryEntity?> CategoryEntities { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}