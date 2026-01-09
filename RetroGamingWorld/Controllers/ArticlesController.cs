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

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("messageArticles"))
            {
                ViewBag.message = TempData["messageArticles"].ToString();
            }

            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            var query = db.Articles
                .Include(a => a.Category)
                .Include(a => a.User)
                .AsQueryable();

            var search = HttpContext.Request.Query["search"].FirstOrDefault();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();

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
                search = "";
            }

            ViewBag.SearchString = search;

            query = query.Where(a => a.IsApproved == true);

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
            var sortCateg = HttpContext.Request.Query["sortCateg"].FirstOrDefault();
            if (!string.IsNullOrEmpty(sortCateg))
            {
                sortCateg = sortCateg.Trim();
            }
            else
            {
                sortCateg = ViewBag.sortCateg ?? "0";
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
            if(sortCateg != "0")
                query = query.Where(art => art.CategoryId == (int)Int32.Parse(sortCateg));

            ViewBag.AllCategories = GetAllCategories();
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.sortCateg = sortCateg;

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
            if (!string.IsNullOrEmpty(search)) 
                ViewBag.PaginationBaseUrl += "search=" + search + "&";
            ViewBag.PaginationBaseUrl += "sortCateg=" + sortCateg + "&sortBy=" + sortBy + "&sortOrder=" + sortOrder + "&page";

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

        [HttpPost]
        [Authorize]
        public IActionResult AddComment([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserID = _userManager.GetUserId(User);
            if (comment.Content is null)
                comment.Content = "";
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

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> PendingArticles()
        {
            var pendingArticles = await db.Articles
                                          .Include(a => a.Category)
                                          .Include(a => a.User)
                                          .Where(a => a.IsApproved == false)
                                          .OrderByDescending(a => a.Date)
                                          .ToListAsync();

            return View(pendingArticles);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var article = db.Articles.Find(id);
            if (article != null)
            {
                article.IsApproved = true;

                article.AdminFeedback = null;

                db.SaveChanges();
                TempData["messageArticles"] = "Articolul \"" + article.Title + "\" a fost aprobat!";
            }

            return RedirectToAction("PendingArticles");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Reject(int id, string reason)
        {
            var article = db.Articles.Find(id);
            if (article != null)
            {
                article.IsApproved = false;
                article.AdminFeedback = reason;

                db.SaveChanges();
                TempData["messageArticles"] = "Articolul a fost respins. Feedback trimis!";
            }

            return RedirectToAction("PendingArticles");
        }

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
        public async Task<IActionResult> New(Article article, IFormFile? ImageFile)
        {
            if (User.IsInRole("Administrator")) return RedirectToAction("Index");

            article.Date = DateTime.Now;
            article.UserId = _userManager.GetUserId(User);
            article.Rating = 0;
            article.IsApproved = false;
            article.AdminFeedback = null;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    TempData["messageArticles"] = "Imaginea este prea mare! Maxim 5MB.";
                    article.Categ = GetAllCategories();
                    return View(article);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["messageArticles"] = "Format invalid! Doar .jpg, .png, .gif";
                    article.Categ = GetAllCategories();
                    return View(article);
                }

                var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                var fileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(storagePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                article.Image = "/images/" + fileName;
            }

            ModelState.Remove(nameof(article.Image));

            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                await db.SaveChangesAsync();
                TempData["messageArticles"] = "Articolul a fost trimis spre aprobare!";

                return RedirectToAction("MyArticles");
            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id, string? source)
        {
            Article article = db.Articles.Include(a => a.Category).FirstOrDefault(a => a.Id == id);
            if (article == null) return NotFound();

            if (!User.IsInRole("Administrator") && article.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            article.Categ = GetAllCategories();

            ViewBag.Source = source;

            return View(article);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Article requestArt, IFormFile? ImageFile, string? source)
        {
            Article art = db.Articles.Find(id);
            if (art == null) return NotFound();

            if (!User.IsInRole("Administrator") && art.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            ModelState.Remove("UserId");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                art.Title = requestArt.Title;
                art.Content = requestArt.Content;
                art.Price = requestArt.Price;
                art.Stock = requestArt.Stock;
                art.CategoryId = requestArt.CategoryId;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["messageArticles"] = "Imaginea este prea mare! Maxim 5MB.";
                        requestArt.Categ = GetAllCategories();
                        ViewBag.Source = source;
                        return View(requestArt);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["messageArticles"] = "Format invalid! Doar .jpg, .png, .gif";
                        requestArt.Categ = GetAllCategories();
                        ViewBag.Source = source;
                        return View(requestArt);
                    }

                    var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                    var fileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                    var filePath = Path.Combine(storagePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    art.Image = "/images/" + fileName;
                }
                if (User.IsInRole("Administrator"))
                {
                    TempData["messageArticles"] = "Articolul a fost modificat!";
                }
                else
                {
                    art.IsApproved = false;
                    art.AdminFeedback = null;
                    TempData["messageArticles"] = "Articolul modificat așteaptă aprobare!";
                }

                db.SaveChanges();

                if (source == "MyArticles") return RedirectToAction("MyArticles");
                if (source == "Show") return RedirectToAction("Show", new { id = id });

                return RedirectToAction("Index");
            }
            else
            {
                requestArt.Categ = GetAllCategories();
                ViewBag.Source = source;
                return View(requestArt);
            }
        }

        [Authorize(Roles = "Administrator,Colaborator")]
        [HttpPost]
        public IActionResult Delete(int id, string? source)
        {
            Article article = db.Articles.Find(id);

            if (article != null)
            {
                if (!User.IsInRole("Administrator") && article.UserId != _userManager.GetUserId(User))
                {
                    TempData["messageArticles"] = "Nu ai dreptul să ștergi acest articol!";
                    return RedirectToAction("Index");
                }

                db.Articles.Remove(article);
                db.SaveChanges();
                TempData["messageArticles"] = "Articolul a fost șters!";
            }

            if (source == "MyArticles")
            {
                return RedirectToAction("MyArticles");
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

        [Authorize(Roles = "Colaborator")]
        [HttpGet]
        public IActionResult MyArticles()
        {
            var userId = _userManager.GetUserId(User);

            var articles = db.Articles
                             .Include(a => a.Category)
                             .Where(a => a.UserId == userId)
                             .OrderByDescending(a => a.Date)
                             .ToList();

            return View(articles);
        }

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