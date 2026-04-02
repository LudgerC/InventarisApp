using InventarisApp.Database;
using InventarisApp.Models;
using InventarisApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
    [Authorize]
    public class DeviceController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly InventarisContext _context;

        public DeviceController(IDeviceService deviceService, InventarisContext context)
        {
            _deviceService = deviceService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var devices = await _deviceService.GetAllDevicesAsync();
            return View(devices);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Locaties = await _context.Locaties.ToListAsync();
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
            ViewBag.DeviceTypes = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Info info, string? mac_address, string? local_ip)
        {
            // Remove navigation properties from validation as they are handled server-side
            ModelState.Remove("Device");
            ModelState.Remove("Lokaal");
            ModelState.Remove("info.Device");
            ModelState.Remove("info.Lokaal");
            
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(mac_address) || !string.IsNullOrEmpty(local_ip))
                {
                    info.Wifis.Add(new Wifi
                    {
                        mac_address = mac_address,
                        local_ip = local_ip,
                        type = info.type,
                        device_id = info.device_id
                    });
                }
                
                bool result = await _deviceService.AddDeviceAsync(info);
                if (result)
                {
                    TempData["Success"] = "Apparaat succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Fout bij het opslaan: Database kon de gegevens niet verwerken.";
            }
            else
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = $"Validatie fout: {errors}";
            }
            ViewBag.Locaties = await _context.Locaties.ToListAsync();
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
            ViewBag.DeviceTypes = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View(info);
        }

        public async Task<IActionResult> Edit(string type, int deviceId)
        {
            if (string.IsNullOrEmpty(type) || deviceId == 0)
            {
                return NotFound();
            }

            var info = await _deviceService.GetDeviceByIdAsync(type, deviceId);
            if (info == null)
            {
                return NotFound();
            }

            ViewBag.Locaties = await _context.Locaties.ToListAsync();
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
            ViewBag.DeviceTypes = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string type, int device_id, Info info)
        {
            if (type != info.type || device_id != info.device_id)
            {
                return NotFound();
            }

            ModelState.Remove("Device");
            ModelState.Remove("Lokaal");
            ModelState.Remove("info.Device");
            ModelState.Remove("info.Lokaal");

            if (ModelState.IsValid)
            {
                bool result = await _deviceService.UpdateDeviceAsync(info);
                if (result)
                {
                    TempData["Success"] = "Apparaat succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Fout bij het bijwerken in de database.";
            }
            else
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = $"Validatie fout: {errors}";
            }
            ViewBag.Locaties = await _context.Locaties.ToListAsync();
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
            ViewBag.DeviceTypes = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string type, int deviceId)
        {
            bool result = await _deviceService.DeleteDeviceAsync(type, deviceId);
            return RedirectToAction(nameof(Index));
        }
    }
}
