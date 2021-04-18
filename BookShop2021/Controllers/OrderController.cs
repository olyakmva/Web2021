using System;
using System.Collections.Generic;
using System.Linq;
using BookShop2021.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Controllers
{
    public class OrderController : Controller
    {
        BookContext db;
        public OrderController(BookContext context)
        {
            db = context;
        }
        string GetCooki(string key)
        {
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains(key))
            {
                return HttpContext.Request.Cookies[key];
            }
            return null;
        }
        public IActionResult Create()
        {
            string cartId = GetCooki("CartId");
            if (cartId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var items = db.ShoppingCarts.Where(c => string.Compare(c.CartId, cartId, StringComparison.Ordinal) == 0).ToList();
            Order newOrder = new Order();
            List<Item> goods = new List<Item>();
            int price = 0;
            foreach (var i in items)
            {
                var b = db.Books.Find(i.BookId);
                if (b == null) continue;
                var tm = new Item() { BookId = i.BookId, Quantity = i.Quantity, TheBook = b };
                price += b.Price * i.Quantity;
                goods.Add(tm);
            }

            newOrder.TotalPrice = price;
            newOrder.Items = goods;
            newOrder.Date = DateTime.Now;
            newOrder.Status = "не подтвержден";
            newOrder.DeliveryMethod = "Самовывоз";
            newOrder.LastName = "Введите фамилию";
            newOrder.Name = "Введите имя";
            newOrder.Items = goods;
            db.Orders.Add(newOrder);
            db.Items.AddRange(goods);
            db.SaveChanges();
          
            return View(newOrder);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,Name,LastName,TotalPrice,Status,DeliveryMethod,Date,Address")]Order order)
        {
            if (ModelState.IsValid)
            {
                order.Status = "подтвержден";
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                RemoveCartRecords();
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        private void RemoveCartRecords()
        {
            string cartId = GetCooki("CartId");
            if (cartId == null) return;
            var carts = db.ShoppingCarts.Where(x => x.CartId == cartId);
            db.ShoppingCarts.RemoveRange(carts);
            db.SaveChanges();
            HttpContext.Response.Cookies.Delete("CartId");
        }

        // GET: /Order/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            var items = db.Items.Where(x => x.OrderId == order.Id);
            var list = items.ToList();
            foreach (var item in list)
            {
                var book = db.Books.Find(item.BookId);
                if (book != null)
                {
                    book.Number += item.Quantity;
                    db.Entry(book).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

    }
}