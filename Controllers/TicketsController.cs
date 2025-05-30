using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CET322.Data;
using CET322.Models;
using CET322.Services;

namespace CET322.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var baseQuery = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Stage)
                .Include(t => t.Company)
                .Include(t => t.Owner)
                .Include(t => t.AssignedTo);

            var tickets = isAdmin
                ? baseQuery
                : baseQuery.Where(t => t.OwnerId == userId || t.AssignedToId == userId);

            return View(await tickets.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Stage)
                .Include(t => t.Company)
                .Include(t => t.Owner)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var allowed = User.IsInRole("Admin") || ticket.OwnerId == userId || ticket.AssignedToId == userId;

            if (!allowed) return Forbid();

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var ticket = new Ticket
            {
                OwnerId = currentUser.Id,
                CompanyId = currentUser.CompanyId ?? 0
            };

            await PrepareViewData(ticket);
            return View(ticket);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.OwnerId = currentUser!.Id;
            ticket.CompanyId = currentUser.CompanyId ?? 0;
            ticket.Status ??= "Açık";

            if (!User.IsInRole("Admin"))
                ticket.AssignedToId = null;

            ModelState.Remove("Category");
            ModelState.Remove("Company");
            ModelState.Remove("Stage");
            ModelState.Remove("Owner");
            ModelState.Remove("AssignedTo");
            ModelState.Remove("OwnerId");

            if (!ModelState.IsValid)
            {
                await PrepareViewData(ticket);
                return View(ticket);
            }

            ticket.CreatedAt = DateTime.Now;
            _context.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = ticket.OwnerId == currentUserId;

            if (!isAdmin && !isOwner)
                return Forbid();

            await PrepareViewData(ticket);
            return View(ticket);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            var dbTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (dbTicket == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = dbTicket.OwnerId == currentUserId;

            if (!isAdmin && !isOwner)
                return Forbid();

            ModelState.Remove("Category");
            ModelState.Remove("Stage");
            ModelState.Remove("Company");
            ModelState.Remove("CompanyId");
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");
            ModelState.Remove("AssignedTo");
            ModelState.Remove("AssignedToId");
            ModelState.Remove("Priority");
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                await PrepareViewData(ticket);
                return View(ticket);
            }

            ticket.CompanyId = dbTicket.CompanyId;
            ticket.OwnerId = dbTicket.OwnerId;

            if (!isAdmin)
            {
                ticket.AssignedToId = dbTicket.AssignedToId;
                ticket.Status = dbTicket.Status;
                ticket.CategoryId = dbTicket.CategoryId;
                ticket.StageId = dbTicket.StageId;
                ticket.Priority = dbTicket.Priority;
            }

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            // Ticket kapandıysa mail gönder
            if (ticket.Status == "Kapalı")
            {
                var owner = await _userManager.FindByIdAsync(ticket.OwnerId);
                if (owner != null)
                {
                    await _emailService.SendEmailAsync(
                        owner.Email,
                        "Ticket Durumu Kapalı olarak güncellendi",
                        $"Merhaba {owner.UserName},\n\n#{ticket.Id} numaralı destek talebiniz 'Kapalı' olarak güncellendi."
                    );
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Stage)
                .Include(t => t.Company)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Comments)
                .Include(t => t.Notes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            _context.Comments.RemoveRange(ticket.Comments);
            _context.TicketNotes.RemoveRange(ticket.Notes);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareViewData(Ticket ticket)
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", ticket.CategoryId);
            ViewData["Companies"] = new SelectList(_context.Companies, "Id", "Name", ticket.CompanyId);
            ViewData["Owners"] = new SelectList(_context.Users, "Id", "Email", ticket.OwnerId);
            ViewData["Stages"] = new SelectList(_context.Stages.Where(s => s.CategoryId == ticket.CategoryId), "Id", "Name", ticket.StageId);
            ViewData["Priorities"] = new SelectList(new[] { "Düşük", "Orta", "Yüksek", "Acil" }, ticket.Priority);
            ViewData["Statuses"] = new SelectList(new[] { "Açık", "Devam Ediyor", "Beklemede", "Kapalı" }, ticket.Status);

            var users = await _userManager.Users.ToListAsync();
            var admins = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    admins.Add(user);
            }
            ViewData["Admins"] = new SelectList(admins, "Id", "Email", ticket.AssignedToId);
        }
    }
}
