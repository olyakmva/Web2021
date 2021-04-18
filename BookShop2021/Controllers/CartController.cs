using System;
using System.Collections.Generic;
using System.Linq;
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
            if(HttpContext.Request.Cookies.Keys.Count >0 &&
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
            if(res.Any())
            {
                CartItem item = res.First();
                item.Quantity++;
                db.Entry(item).State = EntityState.Modified;
            }
            else
            {
                var item = new CartItem()
                {
                     BookId=Id,
                     CartId = cartId,
                     Quantity=1
                };
                db.ShoppingCarts.Add(item);
            }
            db.SaveChanges();

            int count = 0;
            if (cartId != null)
            {
                count = db.ShoppingCarts.Where(c => c.CartId == cartId).Sum(c=>c.Quantity);
            }
            return RedirectToAction("Index", "Home");
        }

        public class ItemId
        {
            public int id { get; set; }
        }
        [HttpPost]
        public IActionResult AddItem([FromBody]ItemId iid)
        {
            int id = iid.id;
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
                                                  c.BookId == id);
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
                    BookId = id,
                    CartId = cartId,
                    Quantity = 1
                };
                db.ShoppingCarts.Add(item);
            }
            db.SaveChanges();

            int count = 0;
            if (cartId != null)
            {
                count = db.ShoppingCarts.Where(c => c.CartId == cartId).Sum(c => c.Quantity);
            }
            return Json(count);
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
        public IActionResult Index()
        {
            string cart = GetCooki("CartId");
            List<CartItem> items = new List<CartItem>();
            if (cart != null)
            {
                items = db.ShoppingCarts.Where(c => string.Compare(c.CartId, cart, StringComparison.Ordinal) == 0).ToList();
                int sum = 0;
                foreach (var item in items)
                {
                    var book = db.Books.Where(b => b.Id == item.BookId).First();
                    item.SelectBook = book;
                    sum += book.Price * item.Quantity;
                }
                ViewBag.Sum = sum;
                ViewBag.Msg = items.Count == 0 ? "Ваша корзина пуста. Надо туда что-то положить :)" : "Ваши книги";
            }
            else
            {
                ViewBag.Msg = "Ваша корзина пуста. Надо туда что-то положить :)";
            }

            return View(items);
        }
        [HttpGet]
        public ActionResult Delete(int? id)
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
        public ActionResult Delete(int id)
        {
            var cart = db.ShoppingCarts.Find(id);
            db.ShoppingCarts.Remove(cart);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public class ChangeItemQuantityDto
        {
            public int id { get; set; }
            public int newQuantity { get; set; }
        }

        public class CartChangingResult
        {
            public int Delta { get; set; }
            public int CartCount { get; set; }
        }

        [HttpPost]
        public ActionResult ChangeItemQuantity([FromBody] ChangeItemQuantityDto dto)
        {
            var cartItem = db.ShoppingCarts.Find(dto.id);
            var book = db.Books.Find(cartItem.BookId);
            var delta = (dto.newQuantity - cartItem.Quantity) * book.Price;
            cartItem.Quantity = dto.newQuantity;
            db.Entry(cartItem).State = EntityState.Modified;
            db.SaveChanges();
            int cartCount = db.ShoppingCarts
                .Where(c => c.CartId == cartItem.CartId)
                .Sum(c => c.Quantity);

            return Json(new CartChangingResult(){CartCount = cartCount, Delta=delta});
        }

       

    }
}