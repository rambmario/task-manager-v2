using Microsoft.AspNetCore.Identity;

namespace TaskManager.App.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
