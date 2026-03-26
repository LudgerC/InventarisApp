using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventarisApp.Database;
using InventarisApp.Models;
using InventarisApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventarisApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly InventarisContext _context;

        public UserController(InventarisContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Deze gebruikersnaam is al in gebruik.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = model.Role,
                    IsActive = true
                };

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent admin from blocking themselves
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "Je kan jezelf niet blokkeren.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string targetRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent admin from changing their own role
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "Je kan je eigen rol niet wijzigen.";
                return RedirectToAction(nameof(Index));
            }

            // If changing from Admin to something else, check if it's the last active admin
            if (user.Role == "Admin" && targetRole != "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);
                if (adminCount <= 1)
                {
                    TempData["Error"] = "Er moet altijd minstens één actieve beheerder (Admin) overblijven.";
                    return RedirectToAction(nameof(Index));
                }
            }

            user.Role = targetRole;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Rol van {user.Username} is gewijzigd naar {targetRole}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
