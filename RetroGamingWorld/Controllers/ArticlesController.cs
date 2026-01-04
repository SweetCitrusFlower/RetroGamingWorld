using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;
using System.Globalization;

namespace RetroGamingWorld.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticlesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        // 1. LISTA DE ARTICOLE (INDEX)
        [HttpGet]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("messageArticles"))
            {
                ViewBag.message = TempData["messageArticles"].ToString();
            }

            var articlesQuery = db.Articles
                                  .Include(a => a.Category)
                                  .Include(a => a.User);
            // LOGICA ADMIN vs USER
            if (User.IsInRole("Administrator"))
            {
                ViewBag.Articles = articlesQuery
                                    .Where(a => a.IsApproved == true || a.IsApproved == false) 
                                    .OrderByDescending(a => a.Id);
            }
            else if (User.IsInRole("Colaborator")){
                ViewBag.Articles = articlesQuery
                                   .Where(a => a.IsApproved == true || a.IsApproved == false)
                                   .OrderByDescending(a => a.Id);
            }
            else
            {
                ViewBag.Articles = articlesQuery
                                    .Where(a => a.IsApproved == true)
                                    .OrderByDescending(a => a.Id);
            }

            var search = "";
            IOrderedQueryable<Article> articles = ViewBag.Articles;

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => c.ArticleId).ToList();

                 List<int> articleIdsOfCategoriesWithSearchString = db.Articles
                                        .Where(a => a.Category.CategoryName.Contains(search))
                                        .Select(a => a.Id).ToList();

                List<int> articleIdsOfCreatorUsersWithSearchString = db.Articles
                                        .Where(a => a.User.FirstName.Contains(search) || a.User.LastName.Contains(search) || a.User.Email.Contains(search))
                                        .Select(a => a.Id).ToList();

                List<int> mergedIds = articleIds
                    .Union(articleIdsOfCommentsWithSearchString)
                    .Union(articleIdsOfCategoriesWithSearchString)
                    .Union(articleIdsOfCreatorUsersWithSearchString).ToList();


                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include(a => a.Category)
                                      .Include(a => a.User)
                                      .OrderByDescending(a => a.Date);

            }
            var sortBy = "";
            if (Convert.ToString(HttpContext.Request.Query["sortBy"]) != null)
                sortBy = Convert.ToString(HttpContext.Request.Query["sortBy"]).Trim();
            else
                if (ViewBag.sortBy is not null)
                    sortBy = ViewBag.sortBy;
                else
                    sortBy = "title";

            var sortOrder = "";
            if (Convert.ToString(HttpContext.Request.Query["sortOrder"]) != null)
                sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]).Trim();
            else if (ViewBag.sortOrder is not null)
                    sortOrder = ViewBag.sortOrder;
                else
                    sortOrder = "asc";

            switch (sortBy)
            {
                case "price":
                    articles = sortOrder == "desc"
                        ? articles.OrderByDescending(a => a.Price)
                        : articles.OrderBy(a => a.Price);
                    break;

                case "rating":
                    articles = sortOrder == "desc"
                        ? articles.OrderByDescending(a => a.Rating)
                        : articles.OrderBy(a => a.Rating);
                    break;

                default:
                    articles = articles.OrderBy(a => a.Title);
                    break;
            }

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            ViewBag.SearchString = search;

            int _perPage = 3;

            int totalItems = articles.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;


            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedArticles = articles.Skip(offset).Take(_perPage);


            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);


            ViewBag.PaginationBaseUrl = "/Articles/Index/";
            if (search != string.Empty && search != null)
            {
                ViewBag.PaginationBaseUrl += "?search=" + search + "&";
            }
            else
            {
                ViewBag.PaginationBaseUrl += "?";
            }
            ViewBag.PaginationBaseUrl += "sortBy=" + sortBy.ToString() + "&sortOrder=" + sortOrder.ToString() + "&page";

            ViewBag.articles = paginatedArticles;
            ViewBag.nrArticles = articles.Count();
            ViewBag.CurrentPage = currentPage;
            return View();
        }

        // 2. AFISAREA UNUI ARTICOL
        [HttpGet]
        public IActionResult Show(int id)
        {
            Article? article = db.Articles
                                 .Include(a => a.Category)
                                 .Include(a => a.User)
                                 .Include(a => a.Comments)
                                    .ThenInclude(c => c.User)
                                 .FirstOrDefault(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // 3. ADAUGARE COMENTARIU (NOU)
        [HttpPost]
        [Authorize]
        public IActionResult AddComment([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserID = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                UpdateArticleRating(comment.ArticleId);
                db.SaveChanges();
                return RedirectToAction("Show", new { id = comment.ArticleId });
            }

            return RedirectToAction("Show", new { id = comment.ArticleId });
        }

        // 4. APROBARE (DOAR ADMIN)
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var article = db.Articles.Find(id);
            if (article != null)
            {
                article.IsApproved = true;
                db.SaveChanges();
                TempData["messageArticles"] = "Articolul \"" + article.Title + "\" a fost aprobat!";
            }
            return RedirectToAction("Index");
        }

        // 5. CREARE ARTICOL (NEW)
        [Authorize(Roles = "Colaborator")]
        [HttpGet]
        public IActionResult New()
        {
            if (User.IsInRole("Administrator"))
            {
                TempData["messageArticles"] = "Adminii nu pot posta articole!";
                return RedirectToAction("Index");
            }

            Article art = new Article();
            art.Categ = GetAllCategories();
            return View(art);
        }

        [Authorize(Roles = "Colaborator")]
        [HttpPost]
        public IActionResult New(Article article)
        {
            if (User.IsInRole("Administrator")) return RedirectToAction("Index");

            article.Date = DateTime.Now;
            article.UserId = _userManager.GetUserId(User);
            article.Rating = 0;
            article.IsApproved = false;

            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["messageArticles"] = "Articolul a fost trimis spre aprobare!";
                return RedirectToAction("Index");
            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        // 6. EDITARE (EDIT)
        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Article article = db.Articles.Include(a => a.Category).FirstOrDefault(a => a.Id == id);
            if (article == null) return NotFound();

            if (!User.IsInRole("Administrator") && article.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            article.Categ = GetAllCategories();
            return View(article);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(int id, Article requestArt)
        {
            Article art = db.Articles.Find(id);
            if (art == null) return NotFound();

            if (ModelState.IsValid)
            {
                art.Title = requestArt.Title;
                art.Content = requestArt.Content;
                art.Image = requestArt.Image;
                art.Price = requestArt.Price;
                art.Stock = requestArt.Stock;
                art.CategoryId = requestArt.CategoryId;

                if (!User.IsInRole("Administrator"))
                {
                    art.IsApproved = false;
                    TempData["messageArticles"] = "Articolul modificat așteaptă aprobare!";
                }
                else
                {
                    TempData["messageArticles"] = "Articolul a fost modificat!";
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                requestArt.Categ = GetAllCategories();
                return View(requestArt);
            }
        }

        // 7. STERGERE (DELETE)
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Find(id);
            if (article != null)
            {
                db.Articles.Remove(article);
                db.SaveChanges();
                TempData["messageArticles"] = "Articolul a fost șters!";
            }
            return RedirectToAction("Index");
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

        // METODA AUXILIARA
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();
            var categories = from cat in db.Categories select cat;

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