using InventarisApp.Database;
using InventarisApp.Models;
using InventarisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventarisApp.Controllers
{
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
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Info info, string mac_address, string local_ip)
        {
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
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to save device.");
            }
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
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

            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
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

            if (ModelState.IsValid)
            {
                bool result = await _deviceService.UpdateDeviceAsync(info);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to update device.");
            }
            ViewBag.Lokalen = await _context.Lokalen.Include(l => l.Locatie).ToListAsync();
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
