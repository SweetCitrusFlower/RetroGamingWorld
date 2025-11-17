using System.ComponentModel.DataAnnotations;

namespace RetroGamingWorld.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }

    }
}
