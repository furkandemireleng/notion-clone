using System.ComponentModel.DataAnnotations;
using notion_clone.Data.Entity.Model;

namespace notion_clone.Dto.Category;

public class PostUpdateDto
{
    [Required] public Guid Id { get; set; }

    public string? Title { get; set; }
    public string? Content { get; set; }

    public List<CategoryEntity> CategoryEntities { get; set; }
}