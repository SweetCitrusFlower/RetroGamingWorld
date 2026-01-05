using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    public class CommentsController(AppDbContext context) : Controller
    {
        private readonly AppDbContext db = context;

        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }
            else
            {
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            db.Comments.Remove(comm);
            db.SaveChanges();
            UpdateArticleRating(comm.ArticleId);
            db.SaveChanges();
            return Redirect("/Articles/Show/" + comm.ArticleId);
        }
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            return View(comm);
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);
            if (ModelState.IsValid)
            {
                comm.Rating = requestComment.Rating;
                comm.Content = requestComment.Content;

                db.SaveChanges();
                UpdateArticleRating(comm.ArticleId);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }
            else
            {
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

        }
        private void UpdateArticleRating(int ArticleId)
        {
            var Article = db.Articles
                            .Include(p => p.Comments)
                            .FirstOrDefault(p => p.Id == ArticleId);

            if (Article != null && Article.Comments.Any())
            {
                Article.Rating = (float?)Article.Comments.Average(c => c.Rating);
            }
        }
    }
}