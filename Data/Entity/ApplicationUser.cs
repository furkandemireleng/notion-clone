namespace notion_clone.Data.Entity;
using Microsoft.AspNetCore.Identity;



public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? EmailConfirmationTokenGeneratedAt { get; set; }

    public virtual ICollection<IdentityUserClaim<string>>? Claims { get; set; }
    public virtual ICollection<IdentityUserLogin<string>>? Logins { get; set; }
    public virtual ICollection<IdentityUserToken<string>>? Tokens { get; set; }
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
}