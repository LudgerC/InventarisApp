using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventarisApp.Services
{
    public interface IDeviceService
    {
        Task<List<Info>> GetAllDevicesAsync();
        Task<Info> GetDeviceByIdAsync(string type, int deviceId);
        Task<bool> AddDeviceAsync(Info info);
        Task<bool> UpdateDeviceAsync(Info info);
        Task<bool> DeleteDeviceAsync(string type, int deviceId);
    }

    public class DeviceService : IDeviceService
    {
        private readonly InventarisContext _context;

        public DeviceService(InventarisContext context)
        {
            _context = context;
        }

        public async Task<List<Info>> GetAllDevicesAsync()
        {
            return await _context.Infos
                .Include(i => i.Device)
                .Include(i => i.Wifis)
                .ToListAsync();
        }

        public async Task<Info> GetDeviceByIdAsync(string type, int deviceId)
        {
            return await _context.Infos
                .Include(i => i.Device)
                .Include(i => i.Wifis)
                .FirstOrDefaultAsync(i => i.type == type && i.device_id == deviceId);
        }

        public async Task<bool> AddDeviceAsync(Info info)
        {
            try
            {
                // Create a new Device record to get an automatically generated ID
                var device = new Device { type = info.type ?? "Unknown" };
                _context.Devices.Add(device);
                
                // Save to generate the ID
                await _context.SaveChangesAsync();
                
                // Assign the generated ID to the Info record
                info.device_id = device.device_id;
                _context.Infos.Add(info);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log exception if needed (can be seen in debugger/output)
                System.Diagnostics.Debug.WriteLine($"Error adding device: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDeviceAsync(Info info)
        {
            var existingInfo = await _context.Infos
                .Include(i => i.Wifis)
                .FirstOrDefaultAsync(i => i.type == info.type && i.device_id == info.device_id);

            if (existingInfo == null)
            {
                return false;
            }

            // Update allowed fields
            existingInfo.merk = info.merk;
            existingInfo.apparaatnaam = info.apparaatnaam;
            existingInfo.model = info.model;
            existingInfo.ip = info.ip;
            existingInfo.status = info.status;
            existingInfo.serial_number = info.serial_number;
            existingInfo.leverancier = info.leverancier;
            existingInfo.garantie = info.garantie;
            existingInfo.aankoopdatum = info.aankoopdatum;
            existingInfo.eind_garantie = info.eind_garantie;

            // Optional Wifi update logic could go here if needed later

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(string type, int deviceId)
        {
            var info = await _context.Infos
                .Include(i => i.Wifis)
                .FirstOrDefaultAsync(i => i.type == type && i.device_id == deviceId);

            if (info == null)
            {
                return false;
            }

            // Deleting the Info will cascade delete Wifi records (configured in DbContext)
            _context.Infos.Remove(info);

            // Also delete the base Device if this was the only Info tied to it
            var otherInfos = await _context.Infos.AnyAsync(i => i.device_id == deviceId && i.type != type);
            if (!otherInfos)
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device != null)
                {
                    _context.Devices.Remove(device);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
