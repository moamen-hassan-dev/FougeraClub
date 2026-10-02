using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FougeraClub1.Models;

namespace FougeraClub1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public IFormFile? SignatureFile { get; set; }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /Profile/UploadSignature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignature()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (SignatureFile == null || SignatureFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please choose a file before saving.";
                return RedirectToAction(nameof(Index));
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(SignatureFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                TempData["ErrorMessage"] = "Only .png, .jpg, or .jpeg files are allowed.";
                return RedirectToAction(nameof(Index));
            }

            var fileName = $"{user.Id}{ext}";
            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", "signatures");
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await SignatureFile.CopyToAsync(stream);
            }

            user.SignatureImagePath = $"/uploads/signatures/{fileName}";
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Signature saved.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/UpdatePhone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhone(string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.PhoneNumber = phoneNumber;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Phone number updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
            }
            else
            {
                // Collect all Identity errors into one message
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = errors;
            }

            return RedirectToAction(nameof(Index));
        }

    }
}