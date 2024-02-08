using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class SigninDto
{
    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }
    
}
