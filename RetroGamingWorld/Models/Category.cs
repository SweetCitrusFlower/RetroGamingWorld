using System.ComponentModel.DataAnnotations;

namespace RetroGamingWorld.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        [MinLength(3, ErrorMessage = "Lungimea minima trebuie sa fie de 3 caractere")]
        public string CategoryName { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
    }

}
