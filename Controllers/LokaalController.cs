using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize]
    public class LokaalController : Controller
    {
        private readonly InventarisContext _context;

        public LokaalController(InventarisContext context)
        {
            _context = context;
        }

        // GET: Lokaal
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lokalen.Include(l => l.Locatie).ToListAsync());
        }

        // GET: Lokaal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokaal = await _context.Lokalen
                .Include(l => l.Devices)
                .Include(l => l.Materialen)
                    .ThenInclude(m => m.MateriaalType)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (lokaal == null)
            {
                return NotFound();
            }

            // Get available devices (those not currently in a room)
            ViewBag.AvailableDevices = await _context.Infos
                .Where(i => i.LokaalId == null)
                .Select(i => new { 
                    Key = i.type + "|" + i.device_id, 
                    Value = (!string.IsNullOrEmpty(i.apparaatnaam) ? i.apparaatnaam + " - " : "") + $"{i.merk} ({i.type}) | {i.serial_number}" 
                })
                .ToListAsync();

            // Get available material types
            ViewBag.MaterialTypes = await _context.MateriaalTypes.OrderBy(t => t.Naam).ToListAsync();

            return View(lokaal);
        }

        // GET: Lokaal/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Locaties = await _context.Locaties.OrderBy(l => l.Naam).ToListAsync();
            return View();
        }

        // POST: Lokaal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Naam,Beschrijving,AantalPlaatsen,IsExtern,LocatieId")] Lokaal lokaal)
        {
            if (ModelState.IsValid)
            {
                // Logic: If not extern, LocatieId is required
                if (!lokaal.IsExtern && lokaal.LocatieId == null)
                {
                    ModelState.AddModelError("LocatieId", "Campus is verplicht voor interne lokalen.");
                }
                else
                {
                    if (lokaal.IsExtern) lokaal.LocatieId = null;
                    _context.Add(lokaal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.Locaties = await _context.Locaties.OrderBy(l => l.Naam).ToListAsync();
            return View(lokaal);
        }

        // GET: Lokaal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokaal = await _context.Lokalen.FindAsync(id);
            if (lokaal == null)
            {
                return NotFound();
            }
            ViewBag.Locaties = await _context.Locaties.OrderBy(l => l.Naam).ToListAsync();
            return View(lokaal);
        }

        // POST: Lokaal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Naam,Beschrijving,AantalPlaatsen,IsExtern,LocatieId")] Lokaal lokaal)
        {
            if (id != lokaal.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Logic: If not extern, LocatieId is required
                if (!lokaal.IsExtern && lokaal.LocatieId == null)
                {
                    ModelState.AddModelError("LocatieId", "Campus is verplicht voor interne lokalen.");
                }
                else
                {
                    try
                    {
                        if (lokaal.IsExtern) lokaal.LocatieId = null;
                        _context.Update(lokaal);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!LokaalExists(lokaal.ID))
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
            }
            ViewBag.Locaties = await _context.Locaties.OrderBy(l => l.Naam).ToListAsync();
            return View(lokaal);
        }

        // POST: Lokaal/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lokaal = await _context.Lokalen.FindAsync(id);
            if (lokaal != null)
            {
                _context.Lokalen.Remove(lokaal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Lokaal/AddMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int lokaalId, int materiaalTypeId, int aantal)
        {
            if (materiaalTypeId <= 0)
            {
                return RedirectToAction(nameof(Details), new { id = lokaalId });
            }

            var materiaal = new Materiaal
            {
                LokaalId = lokaalId,
                MateriaalTypeId = materiaalTypeId,
                Aantal = aantal
            };

            _context.Materialen.Add(materiaal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = lokaalId });
        }

        // POST: Lokaal/RemoveMaterial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMaterial(int id)
        {
            var materiaal = await _context.Materialen.FindAsync(id);
            int lokaalId = 0;
            if (materiaal != null)
            {
                lokaalId = materiaal.LokaalId;
                _context.Materialen.Remove(materiaal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = lokaalId });
        }

        // POST: Lokaal/AssignDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDevice(int lokaalId, string deviceKey)
        {
            if (string.IsNullOrEmpty(deviceKey))
            {
                return RedirectToAction(nameof(Details), new { id = lokaalId });
            }

            var parts = deviceKey.Split('|');
            if (parts.Length != 2) return BadRequest();

            string type = parts[0];
            int deviceId = int.Parse(parts[1]);

            var info = await _context.Infos.FirstOrDefaultAsync(i => i.type == type && i.device_id == deviceId);
            if (info != null)
            {
                info.LokaalId = lokaalId;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = lokaalId });
        }

        // POST: Lokaal/UnassignDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignDevice(int lokaalId, string type, int deviceId)
        {
            var info = await _context.Infos.FirstOrDefaultAsync(i => i.type == type && i.device_id == deviceId);
            if (info != null && info.LokaalId == lokaalId)
            {
                info.LokaalId = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = lokaalId });
        }

        private bool LokaalExists(int id)
        {
            return _context.Lokalen.Any(e => e.ID == id);
        }
    }
}
