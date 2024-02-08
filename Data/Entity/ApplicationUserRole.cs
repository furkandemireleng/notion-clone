using Microsoft.AspNetCore.Identity;

namespace notion_clone.Data.Entity;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ApplicationRole? Role { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
