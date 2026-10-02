using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FougeraClub1.Data;

namespace FougeraClub1.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalOrders = await _context.purchaseOrders.CountAsync();
            ViewBag.PendingOrders = await _context.purchaseOrders.CountAsync(p => p.Status == "Pending");
            ViewBag.ApprovedOrders = await _context.purchaseOrders.CountAsync(p => p.Status == "Approved");
            ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();

            return View();
        }
    }
}