using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop2021.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Controllers
{
    public class CartController : Controller
    {
        BookContext db;
        public CartController(BookContext context)
        {
            db = context;
        }
        public IActionResult Add(int Id)
        {
            string cartId;
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains("CartId"))
            {
                cartId = HttpContext.Request.Cookies["CartId"];
            }
            else
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Response.Cookies.Append("CartId", cartId);
            }
            //check if book exists in cart
            var res = db.ShoppingCarts.Where(c => c.CartId == cartId &&
            c.BookId == Id);
            if (res.Any())
            {
                CartItem item = res.First();
                item.Quantity++;
                db.Entry(item).State = EntityState.Modified;
            }
            else
            {
                var item = new CartItem()
                {
                    BookId = Id,
                    CartId = cartId,
                    Quantity = 1
                };
                db.ShoppingCarts.Add(item);
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");

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
        public IActionResult Index()
        {
            string cartId = GetCookie("CartId");
            var items = new List<CartItem>();
            if (cartId != null)
            {
                items = db.ShoppingCarts
                    .Where(c => c.CartId == cartId)
                    .ToList();
                int sum = 0;
                foreach (var item in items)
                {
                    var book = db.Books.Find(item.BookId);
                    item.SelectBook = book;
                    sum += book.Price * item.Quantity;
                    ViewBag.Sum = sum;
                }
            }
            ViewBag.Msg = items.Count == 0 ? "Ваша корзина пуста. " +
                    "Надо туда что-то положить :)" : "Ваши книги";
            return View(items);
        }
        public class ChangeItemQuantityDto
        {
            public int id { get; set; }
            public int newQuantity { get; set; }
        }
        public class CartChangingResult
        {
            public int delta { get; set; }
            public int cartCount { get; set; }
        }
        [HttpPost]
        public IActionResult ChangeItemQuantity([FromBody]ChangeItemQuantityDto dto)
        {
            var cartItem = db.ShoppingCarts.Find(dto.id);
            var book = db.Books.Find(cartItem.BookId);

            var delta = (dto.newQuantity - cartItem.Quantity) * book.Price;
            cartItem.Quantity = dto.newQuantity;
            db.Entry(cartItem).State = EntityState.Modified;
            db.SaveChanges();
            int count = db.ShoppingCarts
                .Where(c => c.CartId == cartItem.CartId)
                .Sum(c => c.Quantity);
            
            return Json(new CartChangingResult() { delta = delta, cartCount = count });
        }

        
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id != null)
            {
                var cartItem = db.ShoppingCarts.Find(id);
                cartItem.SelectBook = db.Books.Find(cartItem.BookId);
                return View(cartItem);
            }
            return NotFound();
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var cartItem = db.ShoppingCarts.Find(id);
            db.ShoppingCarts.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}