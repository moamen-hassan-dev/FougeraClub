using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FougeraClub1.Models;

namespace FougeraClub1.Controllers
{
    [Authorize(Roles = "Manager")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Users
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                userRoles[user.Id] = roles.FirstOrDefault() ?? "—";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: /Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Email and password are required.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "A user with that email already exists.";
                return RedirectToAction(nameof(Index));
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["SuccessMessage"] = $"User {email} created with role {role}.";
            }
            else
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = errors;
            }

            return RedirectToAction(nameof(Index));
        }
        // POST: /Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} deleted.";
            }
            else
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = errors;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}