using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class RefreshTokenDto
{
    [Required]
    public string? RefreshToken { get; set; }
}
