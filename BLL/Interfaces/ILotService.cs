using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ILotService
    {
        Task<IEnumerable<LotDto>> GetAllLotsAsync();
        Task<LotDto> GetLotByIdAsync(int id);
        Task<int> CreateLotAsync(LotCreateDto lotDto);
        Task ApproveLotAsync(int lotId, int managerId); // Для логіки підтвердження
    }
}