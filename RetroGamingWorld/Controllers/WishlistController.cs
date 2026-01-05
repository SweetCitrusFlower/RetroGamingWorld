using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.Users
            .Include(u => u.Wishlist) 
            .ThenInclude(a => a.Category) 
            .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

        if (user == null) return View(new List<Article>());

        var wishlistArticles = await _context.Articles
            .Where(a => user.Wishlist.Select(w => w.Id).Contains(a.Id))
            .ToListAsync();

        return View(wishlistArticles);
    }

    [HttpGet]
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

}
