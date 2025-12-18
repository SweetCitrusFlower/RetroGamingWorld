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
                var categ = db.Categories
                                .Where(a => a.CategoryName.ToUpper() == cat.CategoryName.ToUpper())
                                .FirstOrDefault();
                if(categ != null)
                {
                    TempData["messageCategories"] = "Categoria \"" + categ.CategoryName + "\" deja există!";
                    return RedirectToAction("Index");
                }
                else
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                    TempData["messageCategories"] = "Categoria\"" + cat.CategoryName + "\" a fost adăugată!";
                    return RedirectToAction("Index");
                }
            }
            return View(cat);
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Categories = db.Categories;
            return View();
        }

        [HttpGet]
        public IActionResult Show(int id)
        {
            var AIC = db.Articles
                        .Include(a => a.Category)
                        .Include(a => a.User)
                        .Where(a => a.CategoryId == id)
                        .OrderByDescending(a => a.Id);
            ViewBag.ArticlesInCategory = AIC;
            if (AIC.Count() == 0)
                ViewBag.CategoryIsEmpty = "Categoria nu are articole";
            else
                ViewBag.CategoryIsEmpty = null;
            ViewBag.CategoryName = db.Categories.Find(id).CategoryName;
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Category categ = db.Categories.Find(id);
            return View(categ);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category pendingCateg)
        {
            Category categ = db.Categories.Find(id);
            if (ModelState.IsValid)
            {
                var categQuery = db.Categories
                                .Where(a => a.CategoryName.ToUpper() == pendingCateg.CategoryName.ToUpper())
                                .FirstOrDefault();
                if(categQuery != null)
                {
                    TempData["messageCategories"] = "Categoria \"" + categ.CategoryName + "\" a fost editată în \"" + pendingCateg.CategoryName + "\"!";
                    categ.CategoryName = pendingCateg.CategoryName;
                    db.SaveChanges();
                }
                else
                {
                    TempData["messageCategories"] = "Categoria \"" + pendingCateg.CategoryName + "\" deja există";
                }
                return RedirectToAction("Index");
            }
            return View(categ);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Category categ = db.Categories.Find(id);
            db.Categories.Remove(categ);
            db.SaveChanges();
            TempData["messageCategories"] = "Categoria \"" + categ.CategoryName + "\" a fost ștearsă";
            return RedirectToAction("Index");
        }
    }
}