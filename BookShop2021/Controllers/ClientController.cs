using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop2021.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BookShop2021.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private BookContext db;
        public ClientController(UserManager<IdentityUser> manager, BookContext context)
        {
            _userManager = manager;
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            var name = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(name);

            var client = db.Clients.Find(user.Id);
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
                
            }
            return View(client);
        }

        public IActionResult Relax()
        {
            return View();
        }
        public async Task<ActionResult> GetPurchaseHistory()
        {
            var name = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(name);
            var orders = db.Orders.Where(d => d.ClientId == user.Id)
                .OrderByDescending(d => d.Date).ToList();
            var books = new List<Book>();
            foreach (var order in orders)
            {
                var items = db.Items.Where(i => i.OrderId == order.Id)
                    .Include(b => b.TheBook).ToList();
                books.AddRange(items.Select(item => item.TheBook));
            }
            var list = books.Distinct().ToList();
            return View(list);
        }

        public ActionResult LeaveReview(int bookId)
        {
            Book book = db.Books.Find(bookId);
            if (book == null)
            {
                return RedirectToAction("Index","Home");
            }
            book.Category = db.Categories.Find(book.CategoryId);
            ViewBag.Book = book;
            var review = new Review { BookId = book.Id };
            return View(review);
        }

        [HttpPost]
        public ActionResult LeaveReview([Bind( "Id,BookId,ClientId,Text")] Review review)
        {
            if (!ModelState.IsValid) return View();
            var client = db.Clients.Find(review.ClientId);
            if (client != null)
            {
                client.ReviewsNumber++;
                db.Entry(client).State = EntityState.Modified;
            }
            db.Reviews.Add(review);
            db.SaveChanges();
            return RedirectToAction("GetPurchaseHistory", "Client");
        }
    }
}