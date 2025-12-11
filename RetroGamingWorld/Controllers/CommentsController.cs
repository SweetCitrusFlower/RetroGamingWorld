using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Mvc;
using RetroGamingWorld.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

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
            Comment? comm = db.Comments.Find(id);
            if (comm is not null)
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Comment? comm = db.Comments.Include(c => c.Article)
                                .FirstOrDefault(c => c.Id == id);
            Console.WriteLine(comm);
            if (comm is null)
                return NotFound();

            return View(comm);
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment? comm = db.Comments.Find(id);
            if (ModelState.IsValid)
            {
                comm.Date = DateTime.Now;
                comm.Content = requestComment.Content;

                db.SaveChanges();
            }
            return Redirect("/Articles/Show/" + comm.ArticleId);
        }
    }
}