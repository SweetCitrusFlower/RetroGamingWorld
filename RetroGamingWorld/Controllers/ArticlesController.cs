using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    // PASUL 10: useri si roluri 
    public class ArticlesController(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly AppDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var articles = db.Articles
                             .Include(a => a.Category)
                             .Include(a => a.User)
                             .OrderByDescending(a => a.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Articles = articles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> articleIds 
                    = db.Articles.Where(at => at.Title.Contains(search)
                                        || at.Content.Contains(search)
                                        ).Select(a => a.Id)
                                        .ToList();

                List<int> articleIdsOfCommentsWithSearchString 
                    = db.Comments
                    .Where(c => c.Content.Contains(search))
                    .Select(c => (int)c.ArticleId)
                    .ToList();

                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();

                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include(a => a.Category)
                                      .Include(a => a.User)
                                      .OrderByDescending(a => a.Date);
            }

            ViewBag.SearchString = search;

            int totalItems = articles.Count();
            int articlesPerPage = 3;
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
                offset = (currentPage - 1) * articlesPerPage;

            var paginatedArticles = articles.Skip(offset).Take(articlesPerPage);

            ViewBag.lastPage = Math.Ceiling((float) totalItems / (float)articlesPerPage);
            ViewBag.Articles = paginatedArticles;

            if (search != "")
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            else
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";

            return View();
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Article? article = db.Articles
                                 .Include(a => a.Category)
                                 .Include(a => a.Comments)
                                 .Include(a => a.User)
                                 .Include(a => a.Comments)
                                    .ThenInclude(c => c.User)
                                 .Where(a => a.Id == id)
                                 .FirstOrDefault();

            if (article is null)
                return NotFound();

            SetAccessRights();

            ViewBag.UserBookmarks = db.Bookmarks
                                      .Where(b => b.UserID == _userManager.GetUserId(User))
                                      .ToList();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddBookmark([FromForm] ArticleBookmark articleBookmark)
        {
            if (ModelState.IsValid)
            {
                if (db.ArticleBookmarks
                    .Where(ab => ab.ArticleId == articleBookmark.ArticleId)
                    .Where(ab => ab.BookmarkId == articleBookmark.BookmarkId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest articol este deja adaugat in colectie";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    db.ArticleBookmarks.Add(articleBookmark);
                    db.SaveChanges();

                    TempData["message"] = "Articolul a fost adaugat in colectia selectata";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga articolul in colectie";
                TempData["messageType"] = "alert-danger";
            }

            return Redirect("/Articles/Show/" + articleBookmark.ArticleId);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New(Article article)
        {
            var sanitizer = new HtmlSanitizer();

            article.Date = DateTime.Now;

            article.UserID = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                article.Content = sanitizer.Sanitize(article.Content);

                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul \"" + article.Title + "\" a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Article? article = db.Articles
                                .Include(a => a.Category)
                                .Where(art => art.Id == id)
                                .FirstOrDefault();

            if (article is null)
            {
                return NotFound();

                // TempData["message"] = "Articolul nu exista!";
                // return RedirectToAction("Index");
            }

            article.Categ = GetAllCategories();

            if ((article.UserID == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                return View(article);
            TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article? article = db.Articles.Find(id);

            if (article is null)
                return NotFound();
            if (ModelState.IsValid)
            {
                if ((article.UserID == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    article.Title = requestArticle.Title;
                    article.Content = requestArticle.Content;
                    article.Date = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    TempData["message"] = "Articolul \"" + article.Title + "\" a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Articles/Show/" + article.Id);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Articles/Show/" + article.Id);
                }
            }
            else
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            // Article article = db.Articles.Find(id);

            Article? article = db.Articles.Include(a => a.Comments)
                                         .Where(art => art.Id == id)
                                         .FirstOrDefault();
            if (article is null)
            {
                return NotFound();
            }

            else
            {
                if ((article.UserID == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
                {
                    db.Articles.Remove(article);
                    db.SaveChanges();
                    TempData["message"] = "Articolul a fost sters";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
        }
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            comment.UserID = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }
            else
            {
                Article? art = db.Articles
                                .Include(a => a.Category)
                                .Include(a => a.User)
                                .Include(a => a.Comments)
                                    .ThenInclude(c => c.User)
                                .Where(art => art.Id == comment.ArticleId)
                                .FirstOrDefault();

                if (art is null)
                {
                    return NotFound();
                }

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                SetAccessRights();

                ViewBag.UserBookmarks = db.Bookmarks
                                          .Where(b => b.UserID == _userManager.GetUserId(User))
                                          .ToList();

                return View(art);
            }

        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            if (User.IsInRole("Editor"))
                ViewBag.AfisareButoane = true;
            ViewBag.UserCurent = _userManager.GetUserId(User);
            ViewBag.EsteAdmin = User.IsInRole("Admin");
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
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/
            return selectList;
        }
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}
