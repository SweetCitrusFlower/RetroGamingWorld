using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // AFISARE
        [HttpGet]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = _context.CartItems
                .Include(c => c.Article)
                .Where(c => c.ApplicationUserId == userId)
                .ToList();

            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"];
            }

            return View(cartItems);
        }

        // ADAUGARE
        [HttpPost]
        public IActionResult Add(int articleId)
        {
            var userId = _userManager.GetUserId(User);
            var article = _context.Articles.Find(articleId);

            if (article == null)
            {
                return NotFound();
            }

            if (article.Stock <= 0)
            {
                TempData["message"] = "Ne pare rău, acest produs nu mai este pe stoc!";
                return RedirectToAction("Index", "Articles");
            }

            var cartItem = _context.CartItems
                .FirstOrDefault(c => c.ApplicationUserId == userId && c.ArticleId == articleId);

            if (cartItem != null)
            {
                if (cartItem.Quantity < article.Stock)
                {
                    cartItem.Quantity++;
                    _context.SaveChanges();
                    TempData["message"] = "Cantitatea produsului a fost actualizată!";
                }
                else
                {
                    TempData["message"] = "Nu poți adăuga mai multe exemplare decât sunt pe stoc!";
                }
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ApplicationUserId = userId,
                    ArticleId = articleId,
                    Quantity = 1,
                    DateCreated = DateTime.Now
                };
                _context.CartItems.Add(newCartItem);
                _context.SaveChanges();
                TempData["message"] = "Produsul a fost adăugat în coș!";
            }

            return RedirectToAction("Index");
        }

        // ACTUALIZARE CANTITATE
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int change)
        {
            var cartItem = _context.CartItems
                                   .Include(c => c.Article)
                                   .FirstOrDefault(c => c.Id == cartItemId);

            var userId = _userManager.GetUserId(User);

            if (cartItem != null && cartItem.ApplicationUserId == userId)
            {
                int newQuantity = cartItem.Quantity + change;

                if (newQuantity >= 1 && newQuantity <= cartItem.Article.Stock)
                {
                    cartItem.Quantity = newQuantity;
                    _context.SaveChanges();
                }
                else if (newQuantity > cartItem.Article.Stock)
                {
                    TempData["message"] = "Nu avem suficient stoc pentru a mări cantitatea!";
                }
            }
            return RedirectToAction("Index");
        }

        // STERGERE
        [HttpPost]
        public IActionResult Remove(int cartItemId)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            var userId = _userManager.GetUserId(User);

            if (cartItem != null && cartItem.ApplicationUserId == userId)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
                TempData["message"] = "Produsul a fost eliminat din coș.";
            }

            return RedirectToAction("Index");
        }

        // PLASARE COMANDA
        [HttpPost]
        public IActionResult PlaceOrder(string firstName, string lastName, string phoneNumber, string county, string city, string addressDetails)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = _context.CartItems
                .Include(c => c.Article)
                .Where(c => c.ApplicationUserId == userId)
                .ToList();

            if (cartItems.Count == 0)
            {
                TempData["message"] = "Coșul tău este gol!";
                return RedirectToAction("Index");
            }

            string fullAddress = $"Destinatar: {firstName} {lastName}\n" +
                                 $"Telefon: {phoneNumber}\n" +
                                 $"Locație: {county}, {city}\n" +
                                 $"Stradă/Detalii: {addressDetails}";

            var order = new Order
            {
                ApplicationUserId = userId,
                Date = DateTime.Now,
                DeliveryAddress = fullAddress,
                Status = "În procesare",
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            decimal finalTotal = 0;

            foreach (var item in cartItems)
            {
                if (item.Article.Stock < item.Quantity)
                {
                    TempData["message"] = $"Stoc insuficient pentru {item.Article.Title}.";
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

                item.Article.Stock -= item.Quantity;

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity,
                    Price = item.Article.Price ?? 0
                };
                _context.OrderDetails.Add(orderDetail);

                finalTotal += item.Quantity * (item.Article.Price ?? 0);
            }

            order.TotalAmount = finalTotal;
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            TempData["OrderSuccess"] = true;
            TempData["LastOrderId"] = order.Id;

            return RedirectToAction("Index");
        }
    }
}