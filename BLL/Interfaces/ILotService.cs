using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ILotService
    {
        Task<IEnumerable<LotDto>> GetAllLotsAsync();
        Task<LotDto> GetLotByIdAsync(int id);
        Task<int> CreateLotAsync(LotCreateDto lotDto);
        Task ApproveLotAsync(int lotId, int managerId);
        Task UpdateLotAsync(int id, LotUpdateDto lotDto);
        Task DeleteLotAsync(int id);

        /// <summary>
        /// Пошук і фільтрація лотів за критеріями
        /// </summary>
        Task<IEnumerable<LotDto>> SearchAndFilterLotsAsync(
            string? searchQuery = null,
            int? categoryId = null,
            LotStatus? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null);
    }
}