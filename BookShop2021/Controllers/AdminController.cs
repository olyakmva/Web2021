using System.Linq;
using System.Threading.Tasks;
using BookShop2021.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShop2021.Controllers
{
    public class AdminController : Controller
    {
        
            //ApplicationDbContext _context;
            //public AdminController(ApplicationDbContext context)
            //{
            //    _context = context;
            //}
            //public IActionResult Index()
            //{
            //    var users = _context.Users.ToList();

            //    return View(users);
            //}
            UserManager<IdentityUser> _userManager;
            RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<IdentityUser> manager, RoleManager<IdentityRole> roleMr)
            {
                _userManager = manager;
                _roleManager = roleMr;
        }
            public IActionResult Index()
            {
                return View(_userManager.Users.ToList());
            }

            public async Task<IActionResult> GetRoles()
            {
                   return View(await _roleManager.Roles.ToListAsync());
            }
            
    }
}
