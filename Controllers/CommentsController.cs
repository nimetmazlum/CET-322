using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CET322.Data;
using CET322.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CET322.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // yorumları listeliyor
        public async Task<IActionResult> Index(int ticketId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.TicketId = ticketId;
            return View(comments);
        }

        
        [HttpGet]
        public IActionResult Create(int ticketId)
        {
            ViewBag.TicketId = ticketId;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("Content", "Yorum boş olamaz.");
                ViewBag.TicketId = ticketId;
                return View();
            }

            var userId = _userManager.GetUserId(User);

            var comment = new Comment
            {
                TicketId = ticketId,
                Content = content,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { ticketId });
        }
    }
}
