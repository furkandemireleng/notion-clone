using System.ComponentModel.DataAnnotations;

namespace notion_clone.Dto;

public class ResendEmailDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
