using Microsoft.AspNetCore.Mvc;
using CET322.Data;
using CET322.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CET322.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stages = await _context.Stages.Include(s => s.Category).ToListAsync();
            return View(stages);
        }
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Stage stage)
        {
            Console.WriteLine($"📥 Form gönderildi. Name: {stage.Name}, CategoryId: {stage.CategoryId}");

            // Navigation property yüzünden model validasyonu bozulabilir, temizliyoruz:
            ModelState.Remove("Category");

            // Model hatalarını yazdıralım
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"❌ ModelState Hatası: {modelState.Key} - {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", stage.CategoryId);
                return View(stage);
            }

            _context.Stages.Add(stage);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Aşama kaydedildi.");
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", stage.CategoryId);
            return View(stage);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Stage stage)
        {
            Console.WriteLine($"✏️ Edit POST - Id: {stage.Id}, Name: {stage.Name}, CategoryId: {stage.CategoryId}");

            ModelState.Remove("Category");

            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"❌ ModelState Hatası: {modelState.Key} - {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", stage.CategoryId);
                return View(stage);
            }

            _context.Stages.Update(stage);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Aşama güncellendi.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var stage = await _context.Stages.Include(s => s.Category)
                                             .FirstOrDefaultAsync(s => s.Id == id);
            if (stage == null)
                return NotFound();

            return View(stage);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage != null)
            {
                _context.Stages.Remove(stage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
