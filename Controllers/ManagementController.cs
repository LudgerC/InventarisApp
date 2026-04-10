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
