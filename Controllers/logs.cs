using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FougeraClub1.Data;

namespace FougeraClub1.Controllers
{
    [Authorize(Roles = "Manager")]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.Logs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }
    }
}