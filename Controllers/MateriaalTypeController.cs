using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MateriaalTypeController : Controller
    {
        private readonly InventarisContext _context;

        public MateriaalTypeController(InventarisContext context)
        {
            _context = context;
        }

        // GET: MateriaalType
        public async Task<IActionResult> Index()
        {
            return View(await _context.MateriaalTypes.ToListAsync());
        }

        // POST: MateriaalType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string naam)
        {
            if (!string.IsNullOrWhiteSpace(naam))
            {
                var type = new MateriaalType { Naam = naam };
                _context.MateriaalTypes.Add(type);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Materiaaltype toegevoegd!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: MateriaalType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string naam)
        {
            var type = await _context.MateriaalTypes.FindAsync(id);
            if (type != null && !string.IsNullOrWhiteSpace(naam))
            {
                type.Naam = naam;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Materiaaltype bijgewerkt!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: MateriaalType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.MateriaalTypes.Include(t => t.Materialen).FirstOrDefaultAsync(t => t.ID == id);
            if (type != null)
            {
                if (type.Materialen.Any())
                {
                    TempData["Error"] = "Dit type kan niet verwijderd worden omdat er nog materialen van dit type zijn.";
                }
                else
                {
                    _context.MateriaalTypes.Remove(type);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Materiaaltype verwijderd!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
