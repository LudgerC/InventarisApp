using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LocatieController : Controller
    {
        private readonly InventarisContext _context;

        public LocatieController(InventarisContext context)
        {
            _context = context;
        }

        // GET: Locatie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Locaties.ToListAsync());
        }

        // POST: Locatie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string naam, string afkorting)
        {
            if (!string.IsNullOrWhiteSpace(naam))
            {
                var locatie = new Locatie { Naam = naam, Afkorting = afkorting };
                _context.Locaties.Add(locatie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Campus toegevoegd!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Locatie/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string naam, string afkorting)
        {
            var locatie = await _context.Locaties.FindAsync(id);
            if (locatie != null && !string.IsNullOrWhiteSpace(naam))
            {
                locatie.Naam = naam;
                locatie.Afkorting = afkorting;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Campus bijgewerkt!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Locatie/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var locatie = await _context.Locaties.Include(l => l.Lokalen).FirstOrDefaultAsync(l => l.ID == id);
            if (locatie != null)
            {
                if (locatie.Lokalen.Any())
                {
                    TempData["Error"] = "Deze campus kan niet verwijderd worden omdat er nog lokalen aan gekoppeld zijn.";
                }
                else
                {
                    _context.Locaties.Remove(locatie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Campus verwijderd!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
