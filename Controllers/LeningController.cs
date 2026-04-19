using InventarisApp.Database;
using InventarisApp.Models;
using InventarisApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize]
    public class LeningController : Controller
    {
        private readonly ILeningService _leningService;
        private readonly InventarisContext _context;

        public LeningController(ILeningService leningService, InventarisContext context)
        {
            _leningService = leningService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var leningen = await _leningService.GetAllLeningenAsync();
            return View(leningen);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return NotFound();

            var lening = await _leningService.GetLeningByIdAsync(id);
            if (lening == null) return NotFound();

            return View(lening);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View(new Lening { startdatum = DateTime.Now.Date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lening lening)
        {
            ModelState.Remove("Persoon");
            ModelState.Remove("Device");

            if (ModelState.IsValid)
            {
                var success = await _leningService.AddLeningAsync(lening);
                if (success)
                {
                    TempData["Success"] = "Lening succesvol aangemaakt!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Fout bij opslaan van de lening in de database.";
            }

            await PopulateDropDownsAsync();
            return View(lening);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return NotFound();

            var lening = await _leningService.GetLeningByIdAsync(id);
            if (lening == null) return NotFound();

            await PopulateDropDownsAsync();
            return View(lening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lening lening)
        {
            if (id != lening.ID) return NotFound();

            ModelState.Remove("Persoon");
            ModelState.Remove("Device");

            if (ModelState.IsValid)
            {
                var success = await _leningService.UpdateLeningAsync(lening);
                if (success)
                {
                    TempData["Success"] = "Lening succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Fout bij het updaten van de lening.";
            }

            await PopulateDropDownsAsync();
            return View(lening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _leningService.DeleteLeningAsync(id);
            if (success) TempData["Success"] = "Lening definitief verwijderd.";
            else TempData["Error"] = "Fout bij het verwijderen van de lening.";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retourneer(int id)
        {
            var success = await _leningService.ReturnLeningAsync(id);
            if (success) TempData["Success"] = "Lening is geretourneerd (afgesloten).";
            else TempData["Error"] = "Fout bij het retourneren van de lening.";
            
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDownsAsync()
        {
            var personen = await _context.Personen
                .OrderBy(p => p.Naam)
                .Select(p => new { p.ID, VolledigeNaam = p.Naam + " " + p.Achternaam + (string.IsNullOrEmpty(p.functie) ? "" : $" ({p.functie})") })
                .ToListAsync();
            
            ViewBag.Personen = new SelectList(personen, "ID", "VolledigeNaam");

            var devices = await _context.Infos
                .Include(i => i.Device)
                .OrderBy(i => i.apparaatnaam)
                .Select(i => new { 
                    i.device_id, 
                    DisplayNaam = $"[{i.type}] " + (!string.IsNullOrEmpty(i.apparaatnaam) ? i.apparaatnaam : $"{i.merk} {i.model}") 
                })
                .ToListAsync();

            ViewBag.Devices = new SelectList(devices, "device_id", "DisplayNaam");
        }
    }
}
