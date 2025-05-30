using Microsoft.AspNetCore.Mvc;
using CET322.Data;
using CET322.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CET322.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            Console.WriteLine("🔔 Form gönderildi. Kategori adı: " + category.Name);

            ModelState.Remove("Stages");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState geçersiz.");
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Kategori veritabanına kaydedildi.");
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            Console.WriteLine($"🎯 EDIT POST - Id: {category.Id}, Name: {category.Name}");

            ModelState.Remove("Stages"); // navigation property varsa

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState geçersiz");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"- Hata - {modelState.Key}: {error.ErrorMessage}");
                    }
                }
                return View(category);
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Güncelleme yapıldı");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}


