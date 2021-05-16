using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using BookShop2021.Models;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Authorization;

namespace BookShop2021.Controllers
{
    [Authorize(Roles = "admin")]
    public class BooksController : Controller
    {
        private readonly BookContext _context;

        IHostingEnvironment _environment;
        public BooksController(BookContext context, IHostingEnvironment env )
        {
            _context = context;
            _environment = env;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var bookContext = _context.Books.Include(b => b.Category);
            return View(await bookContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Author,Name,Price,Year,CategoryId,Number")] Book book,
            IFormFile upload)
        {
            if(upload !=null)
            {
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                //check for graphics
                if (CheckByGraphicsFormat(fileName))
                {
                    Save(upload, fileName);
                    book.ImageUrl = fileName;
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }
        private bool CheckByGraphicsFormat(string fileName)
        {
            var ext = fileName.Substring(fileName.Length - 3);
            return string.CompareOrdinal(ext, "png") == 0 || string.CompareOrdinal(ext, "jpg") == 0;
        }

        private void Save(IFormFile upload, string fileName)
        {
            Bitmap image = new Bitmap(upload.OpenReadStream());
            int width = 150;
            int height = 200;
            var smallImage = Resize(image, width, height);
            string path = "\\wwwroot\\images\\" + fileName;
            var root = _environment.ContentRootPath;
            path = root+ path;
            // сохраняем файл в папку  в каталоге wwwroot
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                smallImage.Save(fileStream, ImageFormat.Jpeg);   
            }
        }
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private Bitmap Resize(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Author,Name,Price,Year,CategoryId,ImageUrl,Number")] Book book,IFormFile upload)
        {
            if (id != book.Id)
            {
                return NotFound();
            }
            if (upload != null)
            {
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                if (CheckByGraphicsFormat(fileName))
                {
                    Save(upload, fileName);
                    book.ImageUrl = fileName;
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
