using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarisApp.Services
{
    public class LeningService : ILeningService
    {
        private readonly InventarisContext _context;

        public LeningService(InventarisContext context)
        {
            _context = context;
        }

        public async Task CleanUpOudeLeningenAsync()
        {
            // Verwijder leningen waarvan de einddatum gesteld is, en die 14 dagen in het verleden ligt.
            var tweeWekenGeleden = DateTime.Now.Date.AddDays(-14);
            var oudeLeningen = await _context.Leningen
                .Where(l => l.einddatum.HasValue && l.einddatum.Value.Date <= tweeWekenGeleden)
                .ToListAsync();

            if (oudeLeningen.Any())
            {
                _context.Leningen.RemoveRange(oudeLeningen);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Lening>> GetAllLeningenAsync()
        {
            // Eerst opkuisen: indien nodig (zorgt ervoor dat oude direct verdwijnen bij ophalen)
            await CleanUpOudeLeningenAsync();

            return await _context.Leningen
                .Include(l => l.Persoon)
                .Include(l => l.Device)
                    .ThenInclude(d => d.Infos)
                .OrderByDescending(l => l.startdatum)
                .ToListAsync();
        }

        public async Task<Lening?> GetLeningByIdAsync(int id)
        {
            return await _context.Leningen
                .Include(l => l.Persoon)
                .Include(l => l.Device)
                    .ThenInclude(d => d.Infos)
                .FirstOrDefaultAsync(l => l.ID == id);
        }

        public async Task<bool> AddLeningAsync(Lening lening)
        {
            try
            {
                _context.Leningen.Add(lening);
                await _context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij AddLening: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateLeningAsync(Lening lening)
        {
            var existing = await _context.Leningen.FindAsync(lening.ID);
            if (existing == null) return false;

            existing.persoonID = lening.persoonID;
            existing.DeviceId = lening.DeviceId;
            existing.startdatum = lening.startdatum;
            existing.einddatum = lening.einddatum;

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

        public async Task<bool> DeleteLeningAsync(int id)
        {
            var lening = await _context.Leningen.FindAsync(id);
            if (lening == null) return false;

            try
            {
                _context.Leningen.Remove(lening);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReturnLeningAsync(int id)
        {
            var lening = await _context.Leningen.FindAsync(id);
            if (lening == null) return false;

            // Voorkom wijziging als ie al in het verleden gestopt is
            if (!lening.einddatum.HasValue || lening.einddatum.Value > DateTime.Now)
            {
                lening.einddatum = DateTime.Now.Date;
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
