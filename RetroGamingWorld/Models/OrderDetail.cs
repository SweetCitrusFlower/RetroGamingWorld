using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetroGamingWorld.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int ArticleId { get; set; }
        public virtual Article? Article { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}