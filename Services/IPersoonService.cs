using System.Collections.Generic;
using System.Threading.Tasks;
using InventarisApp.Models;

namespace InventarisApp.Services
{
    public interface IPersoonService
    {
        Task<List<Persoon>> GetAllPersonenAsync();
        Task<Persoon?> GetPersoonByIdAsync(int id);
        Task<bool> AddPersoonAsync(Persoon persoon);
        Task<bool> UpdatePersoonAsync(Persoon persoon);
        Task<bool> DeletePersoonAsync(int id);
    }
}
