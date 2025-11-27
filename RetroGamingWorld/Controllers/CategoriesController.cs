using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Mvc;
using RetroGamingWorld.Data;

namespace RetroGamingWorld.Controllers
{
    public class CategoriesController(AppDbContext context) : Controller
    {
        private readonly AppDbContext db = context;

        public ActionResult Index()
        {
            if (TempData.ContainsKey("messageCategories"))
            {
                ViewBag.message = TempData["messageCategories"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categories = categories;
            return View();
        }

        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Category cat)
        {
            if(ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["messageCategories"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }
            else
            {
                return View(cat);
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);
            if (ModelState.IsValid)
            { 
                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();
                TempData["messageCategories"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["messageCategories"] = "Categoria a fost stearsa";
            return RedirectToAction("Index");
        }
    }
}
