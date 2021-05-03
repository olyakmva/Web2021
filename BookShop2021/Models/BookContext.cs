using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Models
{
    public class BookContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<CartItem> ShoppingCarts { get; set; }
        public DbSet<Client> Clients { get; set; }
        public BookContext(DbContextOptions<BookContext> options)
             : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();   // создаем базу данных при первом обращении
        }

    }
}