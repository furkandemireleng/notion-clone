using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class UserUpdateDto
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? Name { get; set; }
    
}
