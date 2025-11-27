using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Mvc;
using RetroGamingWorld.Data;

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
            return Redirect("/Articles/Show/" + comm.ArticleId);
        }
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            ViewBag.Comment = comm;
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);
            if (ModelState.IsValid)
            {

                comm.Content = requestComment.Content;

                db.SaveChanges();

                return Redirect("/Articles/Show/" + comm.ArticleId);
            }
            else
            {
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

        }
    }
}