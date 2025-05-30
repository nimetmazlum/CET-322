using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CET322.Data;
using System.Threading.Tasks;
using System.Linq;

namespace CET322.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        
        [HttpGet]
        public IActionResult Login()
        {
            // Şirketler dropdown için 
            ViewBag.Companies = _context.Companies.ToList();
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role, int companyId)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Hem rolde hem de doğru şirkette mi?
                var isInRole = await _userManager.IsInRoleAsync(user, role);
                var isInCompany = (user.CompanyId == companyId);

                if (isInRole && isInCompany)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
                    if (result.Succeeded)
                    {
                        // Giriş başarılıysa
                        return RedirectToAction("Index", "Tickets");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu kullanıcı seçili rolde VE/VEYA şirkette değildir.");
                    ViewBag.Companies = _context.Companies.ToList();
                    return View();
                }
            }
            ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
            ViewBag.Companies = _context.Companies.ToList();
            return View();
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Register(
            string firstName, string lastName, string email, string password, string role, int companyId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CompanyId = companyId
            };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.Companies = _context.Companies.ToList();
            return View();
        }

        
      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}


