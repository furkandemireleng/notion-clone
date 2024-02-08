using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    //[Required]
    // public string? RecaptchaResponse { get; set; }
}