using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetroGamingWorld.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        [Required(ErrorMessage = "Adresa de livrare este obligatorie")]
        public string DeliveryAddress { get; set; }

        public decimal TotalAmount { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}