using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto.Category;

public class CreateCategoryDto
{
    [Required] public string Name { get; set; }
}