using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto.Category;

public class CategoryUpdateDto
{
    [Required] public Guid Id { get; set; }

    [Required] public string? Name { get; set; }
    
}