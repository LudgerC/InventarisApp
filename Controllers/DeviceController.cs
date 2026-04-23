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

        public async Task<IActionResult> Index(string searchString, string statusFilter, string typeFilter, string sortOrder)
        {
            var devices = await _deviceService.GetAllDevicesAsync();
            
            IEnumerable<Info> filteredDevices = devices;

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentType"] = typeFilter;
            ViewData["CurrentSort"] = sortOrder;

            ViewBag.Types = devices.Select(d => d.type).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t).ToList();
            ViewBag.Statuses = devices.Select(d => d.status).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();

            if (!string.IsNullOrEmpty(typeFilter))
            {
                filteredDevices = filteredDevices.Where(d => d.type == typeFilter);
            }
            
            if (!string.IsNullOrEmpty(statusFilter))
            {
                filteredDevices = filteredDevices.Where(d => d.status == statusFilter);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                filteredDevices = filteredDevices.Where(d => 
                    (d.merk != null && d.merk.ToLower().Contains(searchString)) ||
                    (d.model != null && d.model.ToLower().Contains(searchString)) ||
                    (d.serial_number != null && d.serial_number.ToLower().Contains(searchString)) ||
                    (d.ip != null && d.ip.ToLower().Contains(searchString)) ||
                    (d.device_id.ToString().Contains(searchString))
                );
            }

            filteredDevices = sortOrder switch
            {
                "type_desc" => filteredDevices.OrderByDescending(d => d.type).ThenBy(d => d.device_id),
                "id" => filteredDevices.OrderBy(d => d.device_id),
                "id_desc" => filteredDevices.OrderByDescending(d => d.device_id),
                "merk" => filteredDevices.OrderBy(d => d.merk).ThenBy(d => d.device_id),
                "merk_desc" => filteredDevices.OrderByDescending(d => d.merk).ThenBy(d => d.device_id),
                _ => filteredDevices.OrderBy(d => d.type).ThenBy(d => d.device_id)
            };

            return View(filteredDevices.ToList());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.DeviceTypes = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Info info, string? mac_address, string? local_ip)
        {
            // Remove navigation properties from validation as they are handled server-side
            ModelState.Remove("Device");
            ModelState.Remove("info.Device");
            
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
            ModelState.Remove("info.Device");

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

        #region Device Type Management
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeviceTypes()
        {
            var types = await _context.Devices.Select(d => d.type).Distinct().OrderBy(t => t).ToListAsync();
            return View(types);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeviceType(string oldType, string newType)
        {
            if (string.IsNullOrWhiteSpace(oldType) || string.IsNullOrWhiteSpace(newType))
            {
                TempData["Error"] = "Ongeldige naam.";
                return RedirectToAction(nameof(DeviceTypes));
            }

            if (oldType == newType) return RedirectToAction(nameof(DeviceTypes));

            if (await _context.Devices.AnyAsync(d => d.type == newType && d.type != oldType))
            {
                TempData["Error"] = "Dit type bestaat al.";
                return RedirectToAction(nameof(DeviceTypes));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // We need to update all tables that use 'type'
                // Since 'type' is part of PK in Infos and FK in Wifis, 
                // we use raw SQL to handle these updates atomically.
                
                // 1. Update Wifis (FK to Infos)
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Wifis SET type = {newType} WHERE type = {oldType}");

                // 2. Update Infos (PK)
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Infos SET type = {newType} WHERE type = {oldType}");

                // 3. Update Devices
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Devices SET type = {newType} WHERE type = {oldType}");

                await transaction.CommitAsync();
                TempData["Success"] = $"Type succesvol gewijzigd van '{oldType}' naar '{newType}'.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Er is een fout opgetreden bij het bijwerken van het type.";
            }

            return RedirectToAction(nameof(DeviceTypes));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
