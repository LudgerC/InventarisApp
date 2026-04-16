using System.Collections.Generic;
using System.Threading.Tasks;
using InventarisApp.Models;

namespace InventarisApp.Services
{
    public interface ILeningService
    {
        Task<List<Lening>> GetAllLeningenAsync();
        Task<Lening?> GetLeningByIdAsync(int id);
        Task<bool> AddLeningAsync(Lening lening);
        Task<bool> UpdateLeningAsync(Lening lening);
        Task<bool> DeleteLeningAsync(int id);
        Task<bool> ReturnLeningAsync(int id);
        Task CleanUpOudeLeningenAsync();
    }
}
