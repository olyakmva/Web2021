using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookShop2021.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace BookShop2021.Controllers
{
    public class HomeController : Controller
    {
        BookContext db;
        public HomeController(BookContext context)
        {
            db = context;
            //Init();
        }

        private void Init()
        {
            var book1 = new Book()
            {
                Author = "Кнут Д.",
                Name = "Искусство программирования",
                Number = 20,
                Price = 2200,
                Year = 2018
            };
            var zanre1 = new Category()
            {
                Name = "программирование"
            };
            book1.Category = zanre1;
            db.Books.Add(book1);
            db.Categories.Add(zanre1);
            var book2 = new Book()
            {
                Author = "Громыко О.",
                Name = "Верные враги",
                Number = 10,
                Price = 400,
                Year = 2017
            };
            var zanre2 = new Category()
            {
                Name = "фантастика"
            };
            book2.Category = zanre2;
            db.Books.Add(book2);
            db.Categories.Add(zanre2);
            db.SaveChanges();
        }

        public IActionResult Index(int categoryId=0)
        {
            List<Book> books;
            if (categoryId >0)
            {
                books = db.Books.Where(b => b.CategoryId == categoryId).ToList();
            }
            else
            {
                books = db.Books.ToList();
            }
            List<Category> Genres = db.Categories.ToList();
            Genres.Insert(0,new Category() { Name = "все", Id = 0 });
            SelectList listItems = new SelectList(Genres, "Id", "Name");
            ViewBag.Genres = listItems;
            return View(books);
        }
        public IActionResult Search(string searchString)
        {
            List<Book> results = new List<Book>();
            if (searchString != null)
            {
                searchString = searchString.ToLower();
                Regex rg = new Regex(@"\w+");
                var matches = rg.Matches(searchString);
               
                foreach (var match in matches)
                {
                    var word = match.ToString();
                    var list = db.Books
                        .Where(b => b.Author.ToLower().Contains(word) ||
                         b.Name.ToLower().Contains(word)).ToList();
                    results.AddRange(list);
                }
            }
            if (results.Count > 0)
            {
                ViewBag.Msg = "Книги по Вашему запросу";
            }
            else
            {
                ViewBag.Msg = "К сожалению, мы ничего не нашли :(. " +
                    "Но Вы можете обратить внимание на наши бестселлеры ";
                var bestsellers = db.Items.GroupBy(c=>c.BookId)
                    .Select(group => new {
                        id = group.Key,
                        Count = group.Count()
                    }).OrderByDescending(x => x.Count).Take(3).ToList();
                foreach(var item in bestsellers)
                {
                    var book = db.Books.Find(item.id);
                    if(book!=null)
                        results.Add(book);
                }
            }
            return View(results);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
