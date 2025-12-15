using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RetroGamingWorld.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<ArticleBookmark> ArticleBookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === DEFINIRE CHEIE PRIMARA COMPUSA ===
            // Corect este sa folosim doar ArticleId si BookmarkId
            modelBuilder.Entity<ArticleBookmark>()
                .HasKey(ab => new { ab.ArticleId, ab.BookmarkId });


            // === DEFINIRE RELATII (Foreign Keys) ===

            // Relatia cu Article
            modelBuilder.Entity<ArticleBookmark>()
                .HasOne(ab => ab.Article)
                .WithMany(a => a.ArticleBookmarks) // <--- Acum va merge, pentru ca am adaugat lista in Article.cs
                .HasForeignKey(ab => ab.ArticleId);

            // Relatia cu Bookmark
            modelBuilder.Entity<ArticleBookmark>()
                .HasOne(ab => ab.Bookmark)
                .WithMany(b => b.ArticleBookmarks) // <--- Asigura-te ca ai lista asta si in Bookmark.cs!
                .HasForeignKey(ab => ab.BookmarkId);
        }
    }
}