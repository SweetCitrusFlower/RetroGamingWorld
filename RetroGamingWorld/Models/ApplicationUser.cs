using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using RetroGamingWorld.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace RetroGamingWorld.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment> Comments { get; set; } = [];
        [InverseProperty("User")]
        public virtual ICollection<Article> Articles { get; set; } = [];
        public virtual ICollection<Article> Wishlist { get; set; } = new List<Article>();
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
