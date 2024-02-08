using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class SignoutDto
{
    [Required]
    public string? RefreshToken { get; set; }
}
