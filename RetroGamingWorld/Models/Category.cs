using System.ComponentModel.DataAnnotations;

namespace RetroGamingWorld.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
    }
}
