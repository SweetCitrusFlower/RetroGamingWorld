using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

            // QUERY DE BAZA
            var query = db.Articles
               .Include(a => a.Category)
               .Include(a => a.User)
               .AsQueryable();

            // 1. SEARCH SAFE
            // Folosim FirstOrDefault() care nu crapa daca parametrul lipseste
            var search = HttpContext.Request.Query["search"].FirstOrDefault();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();

                // Cautare complexa
                List<int> articleIds = db.Articles.Where(at => at.Title.Contains(search) || at.Content.Contains(search)).Select(a => a.Id).ToList();
                List<int> commentIds = db.Comments.Where(c => c.Content.Contains(search)).Select(c => c.ArticleId).ToList();
                List<int> catIds = db.Articles.Where(a => a.Category.CategoryName.Contains(search)).Select(a => a.Id).ToList();
                List<int> userIds = db.Articles.Where(a => a.User.FirstName.Contains(search) || a.User.LastName.Contains(search) || a.User.Email.Contains(search)).Select(a => a.Id).ToList();

                List<int> mergedIds = articleIds
                    .Union(commentIds)
                    .Union(catIds)
                    .Union(userIds)
                    .ToList();

                query = query.Where(article => mergedIds.Contains(article.Id));
            }
            else
            {
                search = ""; // Asiguram ca nu e null pentru View
            }

            ViewBag.SearchString = search;

            // 2. FILTRE ADMIN/USER
            if (User.IsInRole("Administrator") || User.IsInRole("Colaborator"))
            {
                query = query.Where(a => a.IsApproved == true || a.IsApproved == false);
            }
            else
            {
                query = query.Where(a => a.IsApproved == true);
            }

            // 3. SORTARE SAFE
            var sortBy = HttpContext.Request.Query["sortBy"].FirstOrDefault();
            if (!string.IsNullOrEmpty(sortBy))
            {
                sortBy = sortBy.Trim();
            }
            else
            {
                sortBy = ViewBag.sortBy ?? "title";
            }

            var sortOrder = HttpContext.Request.Query["sortOrder"].FirstOrDefault();
            if (!string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = sortOrder.Trim();
            }
            else
            {
                sortOrder = ViewBag.sortOrder ?? "asc";
            }

            switch (sortBy.ToLower())
            {
                case "price":
                    query = sortOrder == "desc" ? query.OrderByDescending(a => a.Price) : query.OrderBy(a => a.Price);
                    break;
                case "rating":
                    query = sortOrder == "desc" ? query.OrderByDescending(a => a.Rating) : query.OrderBy(a => a.Rating);
                    break;
                case "date":
                    query = sortOrder == "desc" ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date);
                    break;
                default:
                    query = sortOrder == "desc" ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title);
                    break;
            }

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            // 4. PAGINARE
            int _perPage = 3;
            int totalItems = query.Count();

            var pageParam = HttpContext.Request.Query["page"].FirstOrDefault();
            int currentPage = 1;
            if (!string.IsNullOrEmpty(pageParam))
            {
                int.TryParse(pageParam, out currentPage);
            }
            if (currentPage < 1) currentPage = 1;

            var offset = (currentPage - 1) * _perPage;

            var paginatedArticles = query.Skip(offset).Take(_perPage).ToList();

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.CurrentPage = currentPage;

            ViewBag.PaginationBaseUrl = "/Articles/Index/?";
            if (!string.IsNullOrEmpty(search)) ViewBag.PaginationBaseUrl += "search=" + search + "&";
            ViewBag.PaginationBaseUrl += "sortBy=" + sortBy + "&sortOrder=" + sortOrder + "&page";

            // 5. WISHLIST SAFE
            List<int> userWishlistIds = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var currentUserId = _userManager.GetUserId(User);

                var userWithWishlist = db.Users
                    .Include(u => u.Wishlist)
                    .FirstOrDefault(u => u.Id == currentUserId);

                if (userWithWishlist != null && userWithWishlist.Wishlist != null)
                {
                    userWishlistIds = userWithWishlist.Wishlist.Select(a => a.Id).ToList();
                }
            }

            ViewBag.UserWishlist = userWishlistIds;
            ViewBag.Articles = paginatedArticles;
            ViewBag.nrArticles = totalItems;

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