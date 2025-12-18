using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext db;

        public CategoriesController(AppDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["messageArticles"] = "Categoria a fost adăugată!";

                return RedirectToAction("Index", "Articles");
            }
            return View(cat);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cat = db.Categories;
            ViewBag.Categories = cat;
            return View();
        }
    }
}