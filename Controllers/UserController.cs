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

        public async Task<IActionResult> Index(string searchString, string roleFilter, string statusFilter, string sortOrder)
        {
            var usersQuery = _context.Users.AsQueryable();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRole"] = roleFilter;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            ViewBag.Roles = await _context.Users.Select(u => u.Role).Distinct().OrderBy(r => r).ToListAsync();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                usersQuery = usersQuery.Where(u => u.Role == roleFilter);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "Active";
                usersQuery = usersQuery.Where(u => u.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                usersQuery = usersQuery.Where(u => u.Username.ToLower().Contains(searchString));
            }

            usersQuery = sortOrder switch
            {
                "name_desc" => usersQuery.OrderByDescending(u => u.Username),
                "role_asc" => usersQuery.OrderBy(u => u.Role).ThenBy(u => u.Username),
                "role_desc" => usersQuery.OrderByDescending(u => u.Role).ThenBy(u => u.Username),
                "status_asc" => usersQuery.OrderBy(u => u.IsActive).ThenBy(u => u.Username),
                "status_desc" => usersQuery.OrderByDescending(u => u.IsActive).ThenBy(u => u.Username),
                _ => usersQuery.OrderBy(u => u.Username)
            };

            return View(await usersQuery.ToListAsync());
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent admin from deleting themselves
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "Je kan jezelf niet verwijderen.";
                return RedirectToAction(nameof(Index));
            }

            // If deleting an Admin, check if it's the last active admin
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);
                if (adminCount <= 1)
                {
                    TempData["Error"] = "Er moet altijd minstens één actieve beheerder (Admin) overblijven.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Gebruiker {user.Username} is succesvol verwijderd.";
            return RedirectToAction(nameof(Index));
        }
    }
}
