using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CET322.Data;
using CET322.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CET322.Controllers
{
    [Route("api/stages")]
    [ApiController]
    public class StagesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StagesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/stages/bycategory/5
        [HttpGet("bycategory/{id:int}")]
        public async Task<ActionResult<IEnumerable<object>>> ByCategory(int id)
        {
            var stages = await _context.Stages
                                       .Where(s => s.CategoryId == id)
                                       .Select(s => new { s.Id, s.Name })
                                       .ToListAsync();

            if (stages.Count == 0)
                return NotFound();  // kategoriye bağlı aşama tanımlı değilse

            return Ok(stages);      // JSON response: [{ id:1, name:"…" }, … ]
        }
    }
}
