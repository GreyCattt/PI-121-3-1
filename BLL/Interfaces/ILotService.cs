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
        
        /// <summary>
        /// Пошук і фільтрація лотів за критеріями
        /// </summary>
        /// <param name="searchQuery">Пошук по назві лота</param>
        /// <param name="categoryId">Фільтр по категорії (опціонально)</param>
        /// <param name="status">Фільтр по статусу (опціонально)</param>
        /// <param name="minPrice">Мінімальна стартова ціна (опціонально)</param>
        /// <param name="maxPrice">Максимальна стартова ціна (опціонально)</param>
        Task<IEnumerable<LotDto>> SearchAndFilterLotsAsync(
            string? searchQuery = null,
            int? categoryId = null,
            LotStatus? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null);
    }
}