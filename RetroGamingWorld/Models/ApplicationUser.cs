using Microsoft.AspNetCore.Identity;

namespace RetroGamingWorld.Models
{
    //pasul 1: useri si roluri
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
