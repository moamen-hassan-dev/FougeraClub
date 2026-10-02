using FougeraClub1.Data;
using FougeraClub1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FougeraClub1.Controllers
{

    [Authorize]

    public class PurchaseOrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PurchaseOrderItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.purchaseOrderItems.Include(p => p.PurchaseOrder);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PurchaseOrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.purchaseOrderItems
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }

            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Create
        public IActionResult Create(int purchaseOrderId)
        {
            var order = _context.purchaseOrders.Find(purchaseOrderId);
            if (order.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Cannot add items to an approved order.";
                return RedirectToAction("Details", "PurchaseOrders", new { id = purchaseOrderId });
            }
            if (order == null)
            {
                return NotFound();
            }

            ViewData["PurchaseOrderId"] = purchaseOrderId;
            ViewData["OrderNumber"] = order.OrderNumber;
            return View();
        }

        // POST: PurchaseOrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PurchaseOrderId,Description,Quantity,UnitPrice")] PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(purchaseOrderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "PurchaseOrders", new { id = purchaseOrderItem.PurchaseOrderId });
            }

            ViewData["PurchaseOrderId"] = purchaseOrderItem.PurchaseOrderId;
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.purchaseOrderItems.FindAsync(id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }
            ViewData["PurchaseOrderId"] = new SelectList(_context.purchaseOrders, "Id", "CreatedByUserId", purchaseOrderItem.PurchaseOrderId);
            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PurchaseOrderId,Description,Quantity,UnitPrice")] PurchaseOrderItem purchaseOrderItem)
        {
            if (id != purchaseOrderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseOrderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderItemExists(purchaseOrderItem.Id))
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
            ViewData["PurchaseOrderId"] = new SelectList(_context.purchaseOrders, "Id", "CreatedByUserId", purchaseOrderItem.PurchaseOrderId);
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.purchaseOrderItems
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }

            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchaseOrderItem = await _context.purchaseOrderItems.FindAsync(id);
            if (purchaseOrderItem != null)
            {
                _context.purchaseOrderItems.Remove(purchaseOrderItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseOrderItemExists(int id)
        {
            return _context.purchaseOrderItems.Any(e => e.Id == id);
        }
    }
}
