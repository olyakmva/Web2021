using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var books = categoryId >0 ? db.Books.Where(b => b.CategoryId == categoryId).ToList() : db.Books.ToList();
            ViewBag.Genres = GetCategoriesList(); 
            ViewBag.Msg = "Наши книги";
            return View(books);
        }

        private SelectList GetCategoriesList()
        {
            List<Category> genres = db.Categories.ToList();
            genres.Insert(0, new Category() {Name = "все", Id = 0});
            SelectList listItems = new SelectList(genres, "Id", "Name");
            return listItems;
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
                results = (from item in results select item).Distinct().ToList();
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
            ViewBag.Genres = GetCategoriesList();
            return View("Index",results);
        }

        public IActionResult About()
        {
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
