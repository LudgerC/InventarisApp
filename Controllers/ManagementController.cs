using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace InventarisApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagementController : Controller
    {
        private readonly InventarisContext _context;

        public ManagementController(InventarisContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Campus Management
        public async Task<IActionResult> Campuses()
        {
            var campuses = await _context.Locaties.OrderBy(c => c.naam).ToListAsync();
            return View(campuses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCampus(Locatie locatie)
        {
            if (ModelState.IsValid)
            {
                _context.Locaties.Add(locatie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Campus succesvol toegevoegd!";
                return RedirectToAction(nameof(Campuses));
            }
            return View("Campuses", await _context.Locaties.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCampus(int id)
        {
            var campus = await _context.Locaties.FindAsync(id);
            if (campus != null)
            {
                // 1. Nullify references in Info
                var affectedInfos = await _context.Infos.Where(i => i.locatie_id == id).ToListAsync();
                foreach (var info in affectedInfos)
                {
                    info.locatie_id = null;
                    info.lokaalnr = null;
                }

                // 2. Delete all Rooms (Lokalen) for this campus
                var rooms = await _context.Lokalen.Where(r => r.locatie_id == id).ToListAsync();
                _context.Lokalen.RemoveRange(rooms);

                // 3. Delete the Campus
                _context.Locaties.Remove(campus);
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Campus en bijbehorende lokalen verwijderd. Gekoppelde apparaten hebben nu geen locatie meer.";
            }
            return RedirectToAction(nameof(Campuses));
        }
        #endregion

        #region Room Management
        public async Task<IActionResult> Rooms()
        {
            ViewBag.Campuses = await _context.Locaties.ToListAsync();
            var rooms = await _context.Lokalen.Include(r => r.Locatie).OrderBy(r => r.Locatie.naam).ThenBy(r => r.lokaalnr).ToListAsync();
            return View(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Lokaal lokaal)
        {
            if (ModelState.IsValid)
            {
                _context.Lokalen.Add(lokaal);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lokaal succesvol toegevoegd!";
                return RedirectToAction(nameof(Rooms));
            }
            ViewBag.Campuses = await _context.Locaties.ToListAsync();
            return View("Rooms", await _context.Lokalen.Include(r => r.Locatie).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int locatieId, string lokaalnr)
        {
            var room = await _context.Lokalen.FirstOrDefaultAsync(r => r.locatie_id == locatieId && r.lokaalnr == lokaalnr);
            if (room != null)
            {
                // 1. Nullify references in Info
                var affectedInfos = await _context.Infos.Where(i => i.locatie_id == locatieId && i.lokaalnr == lokaalnr).ToListAsync();
                foreach (var info in affectedInfos)
                {
                    info.locatie_id = null;
                    info.lokaalnr = null;
                }

                // 2. Delete the Room
                _context.Lokalen.Remove(room);
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lokaal verwijderd. Gekoppelde apparaten hebben nu geen lokaal meer.";
            }
            return RedirectToAction(nameof(Rooms));
        }
        #endregion

        #region Device Type Management
        public async Task<IActionResult> DeviceTypes()
        {
            var types = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View(types);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeviceType(string type)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!await _context.Devices.AnyAsync(d => d.type == type))
                {
                    _context.Devices.Add(new Device { type = type });
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Apparaattype toegevoegd!";
                }
                else
                {
                    TempData["Error"] = "Dit type bestaat al.";
                }
            }
            return RedirectToAction(nameof(DeviceTypes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDeviceType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                TempData["Error"] = "Ongeldig type.";
                return RedirectToAction(nameof(DeviceTypes));
            }

            // 1. Check if ANY devices (Info) are still using this type
            var deviceCount = await _context.Infos.CountAsync(i => i.type == type);
            if (deviceCount > 0)
            {
                TempData["Error"] = $"Je kunt dit type '{type}' niet verwijderen omdat het nog gekoppeld is aan {deviceCount} apparaat/apparaten.";
                return RedirectToAction(nameof(DeviceTypes));
            }

            // 2. If not in use, find and delete the type record
            var typeToDelete = await _context.Devices.FirstOrDefaultAsync(d => d.type == type);
            if (typeToDelete != null)
            {
                _context.Devices.Remove(typeToDelete);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Apparaattype '{type}' succesvol verwijderd.";
            }

            return RedirectToAction(nameof(DeviceTypes));
        }
#endregion
    }
}
