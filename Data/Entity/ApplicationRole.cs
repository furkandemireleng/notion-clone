using Microsoft.AspNetCore.Identity;

namespace notion_clone.Data.Entity;

public class ApplicationRole : IdentityRole
{
    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }

    public ApplicationRole() : base()
    {

    }

    public ApplicationRole(string roleName) : base(roleName)
    {

    }
}
