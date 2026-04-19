using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventarisApp.Database;
using InventarisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarisApp.Services
{
    public class PersoonService : IPersoonService
    {
        private readonly InventarisContext _context;

        public PersoonService(InventarisContext context)
        {
            _context = context;
        }

        public async Task<List<Persoon>> GetAllPersonenAsync()
        {
            return await _context.Personen
                .Include(p => p.Leningen) // Om het aantal leningen te kunnen tonen
                .OrderBy(p => p.Naam).ThenBy(p => p.Achternaam)
                .ToListAsync();
        }

        public async Task<Persoon?> GetPersoonByIdAsync(int id)
        {
            return await _context.Personen
                .Include(p => p.Leningen)
                    .ThenInclude(l => l.Device)
                        .ThenInclude(d => d.Infos)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<bool> AddPersoonAsync(Persoon persoon)
        {
            try
            {
                _context.Personen.Add(persoon);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fout bij toevoegen persoon: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdatePersoonAsync(Persoon persoon)
        {
            var existing = await _context.Personen.FindAsync(persoon.ID);
            if (existing == null) return false;

            existing.Naam = persoon.Naam;
            existing.Achternaam = persoon.Achternaam;
            existing.emailadres = persoon.emailadres;
            existing.tel = persoon.tel;
            existing.functie = persoon.functie;

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

        public async Task<bool> DeletePersoonAsync(int id)
        {
            var persoon = await _context.Personen
                .Include(p => p.Leningen)
                .FirstOrDefaultAsync(p => p.ID == id);
                
            if (persoon == null) return false;

            try
            {
                // Cascade delete: wis ook expliciet alle leningen die gekoppeld zijn aan deze persoon
                if (persoon.Leningen != null && persoon.Leningen.Any())
                {
                    _context.Leningen.RemoveRange(persoon.Leningen);
                }

                _context.Personen.Remove(persoon);
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
