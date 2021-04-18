using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookShop2021.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            Genres.Add(new Category() { Name = "все", Id = 0 });
            SelectList listItems = new SelectList(Genres, "Id", "Name");
            ViewBag.Genres = listItems;
            return View(books);
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
