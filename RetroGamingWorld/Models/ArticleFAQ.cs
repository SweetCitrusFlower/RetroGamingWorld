using System.ComponentModel.DataAnnotations;

namespace RetroGamingWorld.Models
{
    public class ArticleFAQ
    {
        [Key]
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int AskedCount { get; set; } = 1;
    }

}
