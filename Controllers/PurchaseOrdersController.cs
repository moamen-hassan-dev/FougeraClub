using FougeraClub1.Data;
using FougeraClub1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseOrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PurchaseOrders
        // GET: PurchaseOrders
        public async Task<IActionResult> Index(int? supplierId, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.purchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.Items)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(p => p.OrderDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(p => p.OrderDate <= dateTo.Value.Date.AddDays(1).AddSeconds(-1));

            var orders = await query.OrderByDescending(p => p.Id).ToListAsync();

            ViewData["SupplierList"] = new SelectList(_context.Suppliers, "Id", "Name", supplierId);
            ViewData["CurrentStatus"] = status;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // GET: PurchaseOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.purchaseOrders
                .Include(p => p.ApprovedByUser)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // GET: /PurchaseOrders/Print/5
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.purchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            if (order.Status != "Approved")
            {
                TempData["ErrorMessage"] = "This order has not been approved yet and cannot be printed.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }


        // GET: /PurchaseOrders/Approve/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.purchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(currentUser.SignatureImagePath))
            {
                TempData["ErrorMessage"] = "You must upload a signature in your Profile before approving orders.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == "Approved")
            {
                TempData["ErrorMessage"] = "This order is already approved.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // POST: /PurchaseOrders/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var order = await _context.purchaseOrders.FindAsync(id);
            if (order == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(currentUser.SignatureImagePath))
            {
                TempData["ErrorMessage"] = "You must upload a signature in your Profile before approving orders.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == "Approved")
            {
                TempData["ErrorMessage"] = "This order is already approved.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = "Approved";
            order.ApprovedByUserId = _userManager.GetUserId(User);
            order.ApprovedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            _context.Logs.Add(new Log
            {
                UserId = _userManager.GetUserId(User),
                Action = $"Approved Purchase Order #{order.OrderNumber}"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} approved.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PurchaseOrders/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            var vm = new CreatePurchaseOrderViewModel();
            vm.Items.Add(new CreateItemViewModel()); // start with one empty row
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseOrderViewModel vm)
        {


            // Remove any items that were left completely blank
            vm.Items = vm.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.Description))
                .ToList();

            if (vm.Items.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one item.");
            }

            if (ModelState.IsValid)
            {
                var order = new PurchaseOrder
                {
                    OrderNumber = await GenerateOrderNumberAsync(),
                    OrderDate = vm.OrderDate,
                    WithVAT = vm.WithVAT,
                    SupplierId = vm.SupplierId,
                    Status = "Pending",
                    CreatedByUserId = _userManager.GetUserId(User)
                };

                _context.purchaseOrders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in vm.Items)
                {
                    _context.purchaseOrderItems.Add(new PurchaseOrderItem
                    {
                        PurchaseOrderId = order.Id,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }
                await _context.SaveChangesAsync();

                _context.Logs.Add(new Log
                {
                    UserId = _userManager.GetUserId(User),
                    Action = $"Created Purchase Order #{order.Id}"
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Order #{order.OrderNumber} created.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", vm.SupplierId);
            return View(vm);
        }
        // GET: PurchaseOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.purchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            if (purchaseOrder.Status == "Approved")
{
    TempData["ErrorMessage"] = "Approved orders cannot be edited.";
    return RedirectToAction(nameof(Index));
}

            ViewData["ApprovedByUserId"] = new SelectList(_context.Users, "Id", "Id", purchaseOrder.ApprovedByUserId);
            ViewData["CreatedByUserId"] = new SelectList(_context.Users, "Id", "Id", purchaseOrder.CreatedByUserId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", purchaseOrder.SupplierId);
            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderNumber,OrderDate,WithVAT,Status,SupplierId,CreatedByUserId,ApprovedByUserId")] PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
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
            ViewData["ApprovedByUserId"] = new SelectList(_context.Users, "Id", "Id", purchaseOrder.ApprovedByUserId);
            ViewData["CreatedByUserId"] = new SelectList(_context.Users, "Id", "Id", purchaseOrder.CreatedByUserId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", purchaseOrder.SupplierId);
            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.purchaseOrders
                .Include(p => p.ApprovedByUser)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchaseOrder = await _context.purchaseOrders.FindAsync(id);
            if (purchaseOrder != null)
            {
                _context.purchaseOrders.Remove(purchaseOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.purchaseOrders.Any(e => e.Id == id);
        }

        // GET: /PurchaseOrders/ExportExcel
        public async Task<IActionResult> ExportExcel(int? supplierId, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            // Same filtering logic as Index
            var query = _context.purchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.Items)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(p => p.OrderDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(p => p.OrderDate <= dateTo.Value.Date.AddDays(1).AddSeconds(-1));

            var orders = await query.OrderByDescending(p => p.Id).ToListAsync();

            // Build the workbook
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Purchase Orders");

            // Header row
            var headers = new[] { "PO #", "Supplier", "Items", "Total", "Date", "Status", "Created By", "Approved By" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headers[i];
                sheet.Cell(1, i + 1).Style.Font.Bold = true;
                sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            }

            // Data rows
            int row = 2;
            foreach (var order in orders)
            {
                var subtotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
                var total = order.WithVAT ? subtotal * 1.05m : subtotal;

                sheet.Cell(row, 1).Value = "#" + order.OrderNumber;
                sheet.Cell(row, 2).Value = order.Supplier?.Name ?? "";
                sheet.Cell(row, 3).Value = order.Items.Count;
                sheet.Cell(row, 4).Value = total;
                sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(row, 5).Value = order.OrderDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 6).Value = order.Status;
                sheet.Cell(row, 7).Value = order.CreatedByUser?.Email ?? "";
                sheet.Cell(row, 8).Value = order.ApprovedByUser?.Email ?? "—";
                row++;
            }

            sheet.Columns().AdjustToContents();

            // Return as a downloadable file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"PurchaseOrders_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<int> GenerateOrderNumberAsync()
        {
            var max = await _context.purchaseOrders.MaxAsync(p => (int?)p.OrderNumber) ?? 0;
            return max + 1;
        }
    }

}
