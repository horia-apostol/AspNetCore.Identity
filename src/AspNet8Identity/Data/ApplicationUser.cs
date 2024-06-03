using Microsoft.AspNetCore.Identity;

namespace AspNet8Identity.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public sealed class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
