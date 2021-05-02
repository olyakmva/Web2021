using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookShop2021.Models;
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
        string GetCookie(string key)
        {
            string cartId = null;
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains(key))
            {
                cartId = HttpContext.Request.Cookies[key];
            }
            return cartId;
        }
        public IActionResult Create()
        {
            string cartId = GetCookie("CartId");
            if (cartId == null)
                return RedirectToAction("Index", "Home");
            var items = db.ShoppingCarts
                .Where(c => c.CartId == cartId)
                .ToList();
            var order = new Order();
            var goods = new List<Item>();
            int price = 0;
            foreach(var item in items)
            {
                var book = db.Books.Find(item.BookId);
                if (book == null) continue;
                var good = new Item
                {
                    BookId = book.Id,
                    Quantity = item.Quantity,
                    TheBook = book
                };
                goods.Add(good);
                price += book.Price * item.Quantity;
            }
            order.Items = goods;
            order.TotalPrice = price;
            order.Status = "не подтвержден";
            order.LastName = "Введите фамилию";
            order.Name = "Введите имя";
            order.DeliveryMethod = "курьером";
            order.Address = "Введите адрес";
            order.Date = DateTime.Now;
            db.Orders.Add(order);
            db.Items.AddRange(goods);
            db.SaveChanges();
            return View(order);
        }
        [HttpPost]
        public IActionResult Create([Bind("Id,Date,Name,LastName,TotalPrice,Address,DeliveryMethod")]Order order)
        {
            if(ModelState.IsValid)
            {
                order.Status = "подтвержден";
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                ///TODO : decrease book number
                string cartId = GetCookie("CartId");
                if (cartId != null)
                {
                    var carts = db.ShoppingCarts
                        .Where(c => c.CartId == cartId)
                        .ToList();
                    db.ShoppingCarts.RemoveRange(carts);
                    db.SaveChanges();
                    HttpContext.Response.Cookies.Delete("CartId");
                }
                return RedirectToAction("Success");
            }
            return View();
        }
        public IActionResult Success()
        {
            ViewBag.Msg = "Ваш заказ подтвержден и скоро к Вам приедет.";
            return View();
        }

        public IActionResult Delete(int? id)
        {
           
            if (id == null)
                return BadRequest();
            Order order = db.Orders.Find(id);
            if (order == null)
                return NotFound();
            var items = db.Items.Where(x => x.OrderId == id).ToList();
            order.Items = items;
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");

        }


    }
}