using RetroGamingWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RetroGamingWorld.Controllers
{
    public class ArticlesController(AppDbContext context) : Controller
    {
        private readonly AppDbContext db = context;

        // Se afiseaza lista tuturor articolelor impreuna cu categoria 
        // din care fac parte
        // HttpGet implicit
        public IActionResult Index()
        {
            var articles = db.Articles
                             .Include(a => a.Category)
                             .OrderByDescending(a => a.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Articles = articles;

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // HttpGet implicit
        public IActionResult Show(int id)
        {

            Article article = db.Articles
                                .Include(a => a.Category)
                                .Include(a => a.Comments)
                                .Where(a => a.Id == id)
                                .First();

            ViewBag.Article = article;

            ViewBag.Category = article.Category;


            //ViewBag.Category(ViewBag.UnNume) = article.Category (proprietatea Category);




            return View();


            /* Metoda .First() este necesara pentru a obtine un singur articol din colectia returnata de metoda Where. Fara .First(), codul ar returna o colectie de articole care se potrivesc cu criteriul din Where (chiar daca exista un singur articol care indeplineste conditia), in loc sa returneze direct obiectul Article.
             .First() executa query-ul imediat si returneaza primul (si singurul) articol care indeplineste conditia.
             */

        }

        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit

        [HttpGet]
        public IActionResult New()
        {
            Article art = new Article();

            art.Categ = GetAllCategories();

            return View();
        }

        // Se adauga articolul in baza de date
        [HttpPost]
        public IActionResult New(Article article)
        {
            // se adauga data 
            article.Date = DateTime.Now;

            try
            {
                db.Articles.Add(article);
                db.SaveChanges();
                // Aici dorim sa preluam mesajul "Articolul a fost adaugat";
                // Unde se va afisa acest mesaj? 
                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        public IActionResult Edit(int id)
        {

            Article article = db.Articles
                                .Include(a => a.Category)
                                .First(art => art.Id == id);


            // ce trebuie sa returnam in View?
            return View();


            /* Acesta este codul anterior, utilizand variabile ViewBag
             * 
             * 
            ViewBag.Article = article;
            ViewBag.Category = article.Category;

            var categories = from categ in db.Categories
                             select categ;

             ViewBag.Categories = categories;

            */

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]

        // care sunt parametrii de intrare din metoda Edit()?
        public IActionResult Edit()
        {
            // se cauta articolul in baza de date

            try
            {
                // se editeaza articolul din baza de date, primind noile valori (cele venite din formular)
                // commit -> se salveaza obiectul in baza de date
                db.SaveChanges();
                // Cum se afiseaza acest mesaj? "Articolul a fost modificat";
                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                // ce se intampla pe ramura aceasta?
                return View();
            }
        }


        // Se sterge un articol din baza de date 
        [HttpPost]
        // ce parametru primeste metoda Delete()?
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            // Cum se afiseaza acest mesaj? "Articolul a fost sters";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
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


            // returnam lista de categorii
            return selectList;
        }
    }
}

