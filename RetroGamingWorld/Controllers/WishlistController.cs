using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

[Authorize]
public class WishlistController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WishlistController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    [HttpGet]
    public IActionResult Index()
    {
        var currentUserId = _userManager.GetUserId(User);

        var user = _context.Users
            .Include(u => u.Wishlist).ThenInclude(a => a.User)  
            .Include(u => u.Wishlist).ThenInclude(a => a.Category) 
            .Include(u => u.Wishlist).ThenInclude(a => a.Comments)  
            .FirstOrDefault(u => u.Id == currentUserId);

        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var query = user.Wishlist.AsQueryable();

        // SEARCH
        var search = HttpContext.Request.Query["search"].FirstOrDefault();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.Trim();
            var searchTerm = search.ToLower();

            query = query.Where(article =>
                (article.Title != null && article.Title.ToLower().Contains(searchTerm)) ||
                (article.Content != null && article.Content.ToLower().Contains(searchTerm)) ||
                (article.Category != null && article.Category.CategoryName.ToLower().Contains(searchTerm)) ||
                (article.User != null && (article.User.Email.ToLower().Contains(searchTerm) || article.User.UserName.ToLower().Contains(searchTerm))) ||
                (article.Comments != null && article.Comments.Any(c => c.Content.ToLower().Contains(searchTerm)))
            );
        }
        else
        {
            search = "";
        }

        ViewBag.SearchString = search;

        // SORTARE
        var sortBy = HttpContext.Request.Query["sortBy"].FirstOrDefault();
        var sortOrder = HttpContext.Request.Query["sortOrder"].FirstOrDefault();

        if (string.IsNullOrEmpty(sortBy)) sortBy = "title";
        if (string.IsNullOrEmpty(sortOrder)) sortOrder = "asc";

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
                if (sortOrder == "desc")
                    query = query.OrderByDescending(a => a.Price);
                else
                    query = query.OrderBy(a => a.Price);
                break;

            case "rating":
                if (sortOrder == "desc")
                    query = query.OrderByDescending(a => a.Rating);
                else
                    query = query.OrderBy(a => a.Rating);
                break;

            default:
                query = query.OrderByDescending(a => a.Rating);
                break;
        }
        if (sortCateg != "0")
            query = query.Where(art => art.CategoryId == (int)Int32.Parse(sortCateg));

        ViewBag.AllCategories = GetAllCategories();
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder;
        ViewBag.sortCateg = sortCateg;

        // PAGINARE
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
        var paginatedItems = query.Skip(offset).Take(_perPage).ToList();

        ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
        ViewBag.CurrentPage = currentPage;

        string paginationBaseUrl = "/Wishlist/Index/?";
        if (!string.IsNullOrEmpty(search)) paginationBaseUrl += "search=" + search + "&";
        paginationBaseUrl += "sortBy=" + sortBy + "&sortOrder=" + sortOrder + "&page";

        ViewBag.PaginationBaseUrl = paginationBaseUrl;

        return View(paginatedItems);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int articleId)
    {
        var user = await _userManager.Users
            .Include(u => u.Wishlist)
            .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

        var article = await _context.Articles.FindAsync(articleId);

        if (article != null && !user!.Wishlist.Contains(article))
        {
            user.Wishlist.Add(article);
            await _context.SaveChangesAsync();
        }

        return Redirect(Request.Headers["Referer"].ToString());
    }

    [Authorize]
    [HttpPost]
    public IActionResult Remove(int articleId)
    {
        var currentUserId = _userManager.GetUserId(User);

        var user = _context.Users
            .Include(u => u.Wishlist)
            .FirstOrDefault(u => u.Id == currentUserId);

        if (user != null)
        {
            var articleToRemove = user.Wishlist.FirstOrDefault(a => a.Id == articleId);
            if (articleToRemove != null)
            {
                user.Wishlist.Remove(articleToRemove);
                _context.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }

    [NonAction]
    public IEnumerable<SelectListItem> GetAllCategories()
    {
        var selectList = new List<SelectListItem>();
        var categories = from cat in _context.Categories select cat;

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
