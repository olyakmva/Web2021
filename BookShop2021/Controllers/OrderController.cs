using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookShop2021.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Controllers
{
    public class OrderController : Controller
    {
        BookContext db;
        UserManager<IdentityUser> _userManager;

        public OrderController(BookContext context, UserManager<IdentityUser> userManager)
        {
            db = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Create()
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
                if (book.Number < item.Quantity)
                {
                    item.Quantity = book.Number;
                }

                book.Number -= item.Quantity;
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                goods.Add(good);
                price += book.Price * item.Quantity;
            }
            order.Items = goods;
            order.TotalPrice = price;
            order.Status = "не подтвержден";
            order.Date = DateTime.Now;
            order.LastName = "Введите фамилию";
            order.Name = "Введите имя";
            order.DeliveryMethod = "курьером";
            order.Address = "Введите адрес";

            ViewBag.Discount = 0;
            if (User.Identity.IsAuthenticated)
            {
                var name = HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(name);

                var client = db.Clients.Find(user.Id);
                if (client != null)
                {
                    order.ClientId = user.Id;
                    if (client.LastName != null)
                        order.LastName = client.LastName;
                    if (client.Name != null)
                        order.Name = client.Name;
                    if (client.Address != null)
                        order.Address = client.Address;
                    if (client.CurrentDiscount > 0)
                    {
                        var discount = (client.CurrentDiscount * order.TotalPrice) / 100;
                        ViewBag.Discount = discount;
                        ViewBag.Cost = order.TotalPrice;
                        order.TotalPrice -= discount;
                    }
                }
            }
            db.Orders.Add(order);
            db.Items.AddRange(goods);
            db.SaveChanges();
            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Date,Name,LastName,TotalPrice,Address,DeliveryMethod, ClientId")]Order order)
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

                await ChangeDataForAuthUsersAsync(order);
                return RedirectToAction("Success");
            }
            return View();
        }
        public IActionResult Success()
        {
            ViewBag.Msg = "Ваш заказ подтвержден и скоро к Вам приедет.";
            return View();
        }
        private async Task ChangeDataForAuthUsersAsync(Order order)
        {
            if (!User.Identity.IsAuthenticated) return;
            var name = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(name);

            var client = db.Clients.Find(user.Id);
            bool isNew = false;
            if (client == null)
            {
                client = new Client
                {
                    Id = user.Id,
                    OrdersNumber = 0,
                    CurrentDiscount = 0,
                    TotalOrdersCost = 0,
                    ReviewsNumber = 0
                };
                isNew = true;
            }
            client.Address = order.Address;
            client.Name = order.Name;
            client.LastName = order.LastName;
            client.OrdersNumber++;
            client.TotalOrdersCost += order.TotalPrice;
            DiscountCalculator(client);
            if (isNew)
            {
                db.Clients.Add(client);
            }
            else
            {
                db.Entry(client).State = EntityState.Modified;
            }

            db.SaveChanges();
        }

        private static void DiscountCalculator(Client client)
        {
            if (client.TotalOrdersCost > 100000)
            {
                client.CurrentDiscount = 30;
                return;
            }

            if (client.TotalOrdersCost > 50000)
            {
                client.CurrentDiscount = 25;
                return;
            }
            if (client.TotalOrdersCost > 25000 || client.OrdersNumber > 25)
            {
                client.CurrentDiscount = 15;
                return;
            }
            if (client.TotalOrdersCost > 10000 || client.OrdersNumber > 10)
            {
                client.CurrentDiscount = 10;
                return;
            }

            client.CurrentDiscount = 5;
        }
        public IActionResult Delete(int? id)
        {
           
            if (id == null)
                return BadRequest();
            Order order = db.Orders.Find(id);
            if (order == null)
                return NotFound();
            var items = db.Items.Where(x => x.OrderId == id).ToList();
           
            foreach (var item in items)
            {
                var book = db.Books.Find(item.BookId);
                if (book != null)
                {
                    book.Number += item.Quantity;
                    db.Entry(book).State = EntityState.Modified;
                }
            }
            order.Items = items;
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}