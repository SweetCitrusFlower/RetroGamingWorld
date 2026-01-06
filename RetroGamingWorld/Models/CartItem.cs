using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetroGamingWorld.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ArticleId { get; set; }
        public virtual Article? Article { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Cantitatea trebuie să fie cel puțin 1")]
        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; }
    }
}