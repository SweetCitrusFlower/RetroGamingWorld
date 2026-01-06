using System.ComponentModel.DataAnnotations.Schema;

namespace RetroGamingWorld.Models
{
    public class ArticleBookmark
    {
        public int Id { get; set; } 
        public int ArticleId { get; set; }
        public virtual Article? Article { get; set; }
        public int BookmarkId { get; set; }
        public virtual Bookmark? Bookmark { get; set; }
    }
}