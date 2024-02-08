namespace notion_clone.Dto.Category;

public class CategoryResponseDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}