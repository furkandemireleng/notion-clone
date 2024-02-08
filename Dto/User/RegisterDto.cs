using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string? Username { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [Required]
    [MinLength(6)]
    public string? Password { get; set; }
}