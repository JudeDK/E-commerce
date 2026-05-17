using Microsoft.AspNetCore.Identity;

namespace ProiectWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nickname { get; set; }
    }
}
