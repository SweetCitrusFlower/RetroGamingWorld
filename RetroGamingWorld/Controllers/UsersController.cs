using RetroGamingWorld.Data;
using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RetroGamingWorld.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly AppDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var users = from User in db.Users
                        join UserRole in db.UserRoles on User.Id equals UserRole.UserId
                        join Role in db.Roles on UserRole.RoleId equals Role.Id
                        orderby User.Id
                        select new
                        {
                            User,
                            RoleName = Role.NormalizedName
                        };
            ViewBag.UsersList = users;

            return View();
        }

        public async Task<IActionResult> ShowAsync(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);
                 
                ViewBag.Roles = roles;

                ViewBag.UserCurent = await _userManager.GetUserAsync(User);

                return View(user);
            } 
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            else
            {
                ViewBag.AllRoles = GetAllRoles();

                return View(user);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditAsync(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {

                    // Cautam toate rolurile din baza de date
                    var roles = db.Roles.ToList();

                    foreach (var role in roles)
                    {
                        // Scoatem userul din rolurile anterioare
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }

                    // Adaugam noul rol selectat
                    var roleName = await _roleManager.FindByIdAsync(newRole);
                    await _userManager.AddToRoleAsync(user, roleName.ToString());

                    db.SaveChanges();

                }

                user.AllRoles = GetAllRoles();
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {

            var user = db.ApplicationUsers
                            .Include(u => u.Articles)
                            .Include(u => u.Comments)
                            .Where(u => u.Id == id)
                            .First();

            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // Delete user articles
            if (user.Articles.Count > 0)
            {
                foreach (var article in user.Articles)
                {
                    db.Articles.Remove(article);
                }
            }

            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");

        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
