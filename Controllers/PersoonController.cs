using InventarisApp.Models;
using InventarisApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize]
    public class PersoonController : Controller
    {
        private readonly IPersoonService _persoonService;

        public PersoonController(IPersoonService persoonService)
        {
            _persoonService = persoonService;
        }

        public async Task<IActionResult> Index()
        {
            var personen = await _persoonService.GetAllPersonenAsync();
            return View(personen);
        }

        public IActionResult Create()
        {
            return View(new Persoon());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Persoon persoon)
        {
            if (ModelState.IsValid)
            {
                bool success = await _persoonService.AddPersoonAsync(persoon);
                if (success)
                {
                    TempData["Success"] = "Persoon succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Er was een probleem met het opslaan van de persoon in de database.";
            }
            return View(persoon);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0) return NotFound();

            var persoon = await _persoonService.GetPersoonByIdAsync(id);
            if (persoon == null) return NotFound();

            return View(persoon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Persoon persoon)
        {
            if (id != persoon.ID) return NotFound();

            // We verwijderen Leningen uit de ModelState navigatie mocht die zich opdringen
            ModelState.Remove("Leningen");

            if (ModelState.IsValid)
            {
                bool success = await _persoonService.UpdatePersoonAsync(persoon);
                if (success)
                {
                    TempData["Success"] = "Persoon succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Er was een probleem bij het bijwerken van de persoon.";
            }

            // Omdat de Leningen tabel weergegeven moet worden in de Edit view bij een invalid form, 
            // moeten we de de persoon overnieuw ophalen om de leningen weer mee te geven.
            var reloadedPersoon = await _persoonService.GetPersoonByIdAsync(id);
            if (reloadedPersoon != null)
            {
                persoon.Leningen = reloadedPersoon.Leningen;
            }

            return View(persoon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _persoonService.DeletePersoonAsync(id);
            if (success)
            {
                TempData["Success"] = "Persoon (en bijbehorende leningen) definitief verwijderd.";
            }
            else
            {
                TempData["Error"] = "Er was een probleem met het verwijderen van de persoon.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
