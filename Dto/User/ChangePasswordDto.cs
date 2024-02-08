using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class ChangePasswordDto
{
    [Required]
    [EmailAddress]
    public string? Username { get; set; }

    [Required]
    public string? OldPassword { get; set; }

    [Required]
    [MinLength(6)]
    [RegularExpression("(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?!.*([0-9])\\1)^[a-zA-Z0-9]+$")]
    public string? NewPassword { get; set; }
}