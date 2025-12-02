using RetroGamingWorld.Models;
using RetroGamingWorld.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RetroGamingWorld.Controllers
{
    public class ArticlesController(AppDbContext context) : Controller
    {
        private readonly AppDbContext db = context;
        [HttpGet]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("messageArticles"))
            {
                ViewBag.message = TempData["messageArticles"].ToString();
            }
            var articles = db.Articles
                             .Include(a => a.Category)
                             .OrderByDescending(a => a.Id);
            ViewBag.Articles = articles;

            return View();
        }

        [HttpGet]
        public IActionResult Show(int id)
        {
            
            Article? article = db.Articles
                                .Include(a => a.Category)
                                .Include(a => a.Comments)
                                    .ThenInclude(c => c.User)
                                .Include(a => a.User)
                                .Where(a => id == a.Id)
                                .FirstOrDefault();
            if(article is null)
            {
                return NotFound();
            }

            //ViewBag.Article = article;
            //ViewBag.Category = article.Category;
            //ViewBag.Comments = article.Comments;


            return View(article);
        }

        //[HttpPost]
        //public IActionResult Show([FromForm] Comment comment)
        //{
        //    comment.Date = DateTime.Now;
        //    if (ModelState.IsValid)
        //    {
        //        db.Comments.Add(comment);
        //        db.SaveChanges();
        //        return Redirect("Articles/Show/" + comment.ArticleId);
        //    }
        //    else
        //    {
        //        Article? article = db.Articles
        //                        .Include(a => a.Category)
        //                        .Include(a => a.Comments)
        //                        .Where(a => comment.ArticleId == a.Id)
        //                        .First();
        //    }
        //}

        [HttpGet]
        public IActionResult New()
        {
            Article art = new Article();

            art.Categ = GetAllCategories();

            return View(art);
        }

        [HttpPost]
        public IActionResult New(Article article)
        {
            article.Date = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["messageArticles"] = "Articolul \"" + article.Title + "\" a fost adaugat!";
                return RedirectToAction("Index");
            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Article article = db.Articles
                                .Include(a => a.Category)
                                .First(art => art.Id == id);
            article.Categ = GetAllCategories();

            return View(article);
        }

        [HttpPost]
        public IActionResult Edit(int id, Article requestArt)
        {
            Article art = db.Articles.Find(id);
            if (ModelState.IsValid)
            {
                art.Title = requestArt.Title;
                art.Content = requestArt.Content;
                art.CategoryId = requestArt.CategoryId;

                db.SaveChanges();
                TempData["messageArticles"] = "Articolul a fost modificat!";
                return RedirectToAction("Index");

            }
            else
            {
                requestArt.Categ = GetAllCategories();
                return View(requestArt);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            TempData["messageArticles"] = "Articolul a fost sters!";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            return selectList;
        }
    }
}

