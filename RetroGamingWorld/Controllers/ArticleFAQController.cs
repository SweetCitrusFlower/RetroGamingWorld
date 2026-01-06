using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    public class ArticleFAQController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticleFAQController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Ask(int articleId, string question)
        {
            var art = _context.Articles
                .Where(a => a.Id == articleId)
                .FirstOrDefault();

            if (art == null)
            {
                TempData["AIAnswer"] = "Produs inexistent.";
                return RedirectToAction("Show", "Articles", new { id = articleId });
            }

            if (art.Content.ToLower().Contains(question.ToLower()))
            {
                TempData["AIAnswer"] = "Informatia exista deja in descrierea produsului.";
                return RedirectToAction("Show", "Articles", new { id = articleId });
            }

            var faq = _context.ArticleFAQs.
                Where(aFAQ => aFAQ.ArticleId == articleId)
                .FirstOrDefault(f =>
                    question.ToLower().Contains(f.Question.ToLower()) ||
                    f.Question.ToLower().Contains(question.ToLower()));

            if (faq != null)
            {
                faq.AskedCount++;
                _context.SaveChanges();
                
                TempData["AIAnswer"] = faq.Answer;
                return RedirectToAction("Show", "Articles", new { id = articleId });
            }
            TempData["AIAnswer"] = "Momentan nu avem detalii despre acest aspect.";
            return RedirectToAction("Show", "Articles", new { id = articleId });
        }

        public IActionResult CreateFAQ(int articleID, string question, string answer)
        {
            var faq = new ArticleFAQ
            {
                ArticleId = articleID,
                Question = question,
                Answer = answer
            };

            _context.ArticleFAQs.Add(faq);
            _context.SaveChanges();

            return RedirectToAction("FAQs", "Article", new { id = articleID });
        }

    }
}
