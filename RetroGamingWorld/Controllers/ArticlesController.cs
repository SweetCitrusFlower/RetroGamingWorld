using Microsoft.AspNetCore.Mvc;
using RetroGamingWorld.Models;

namespace RetroGamingWorld.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly AppDbContext _db;

        public ArticlesController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            var articles = from article in _db.Articles
                           select article;

            ViewBag.Articles = articles;

            return View();
        }

        public ActionResult Show(int id)
        {
            Article? article = _db.Articles.Find(id);

            ViewBag.Article = article;

            return View();
        }
        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Article art)
        {
            art.Date = DateTime.Now;
            try
            {
                _db.Articles.Add(art);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }


        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Article art = _db.Articles.Find(id);
            ViewBag.Article = art;
            return View();

        }

        //[HttpPost]
        //public ActionResult Edit(int id, Article requestArticle)
        //{

        //}



        [HttpPost]
        public ActionResult Delete(int id)
        {
            Article? article = _db.Articles.Find(id);

            if (article != null)
            {
                _db.Articles.Remove(article);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
    }
}
