using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookShop2021.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Controllers
{

    public class ManagerController : Controller
    {
        BookContext _db;
        public ManagerController(BookContext context)
        {
            _db = context;
        }

        public IActionResult GetOrdersNumber()
        {
            OrderNumberViewModel model = new OrderNumberViewModel();
            model.End = DateTime.Now;
            model.Start = new DateTime(2021, 02, 10);
            return View(model);
        }
        [HttpPost]
        public IActionResult GetOrdersNumber([Bind("Start,End")]OrderNumberViewModel model)
        {
            var total = _db.Orders
                .Where(o => o.Date >= model.Start && o.Date <= model.End)
                .Sum(o => o.TotalPrice);
            var count = _db.Orders
                .Count(o => o.Date >= model.Start && o.Date <= model.End);
            model.Number = count;
            model.TotalSum = total;
            return View(model); 
        }
        public IActionResult GetSmallNumberOfBooks(int count = 5)
        {
            const int maxMinBooksQuantity = 25;
            List<Book> books =
                _db.Books.Include(b=>b.Category)
                .Where(b=>b.Number<=count).ToList();
           
            List<int> list = new List<int>();
            for (int i = 5; i < maxMinBooksQuantity; i += 5)
                list.Add(i);
            SelectList listItems = new SelectList(list);
            ViewBag.MinQuantity = listItems;
            return View(books);
        }

        public IActionResult GetBestsellerList()
        {
            var orders = _db.Orders.ToList();
            var booksDictionary = new Dictionary<int, int>();
            foreach (var order in orders)
            {
                var books = _db.Items.Where(i => i.OrderId == order.Id).ToList();
                foreach (var book in books)
                {
                    if (booksDictionary.ContainsKey(book.BookId))
                    {
                        booksDictionary[book.BookId] += book.Quantity;
                    }
                    else booksDictionary.Add(book.BookId, book.Quantity);
                }
            }
            var result = (from t in booksDictionary orderby t.Value descending select t).Take(5);

            List<Bestseller> list = new List<Bestseller>();

            foreach (var item in result)
            {
                var book = _db.Books.Find(item.Key);
                if (book == null) continue;
                var zanr = _db.Categories.Find(book.CategoryId);
                book.Category = zanr;
                var bestseller = new Bestseller() { TheBook = book, Quantity = item.Value };
                list.Add(bestseller);
            }

            return View(list);
        }
    }
}