using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAuctionService
    {
        // Метод для розміщення ставки
        Task<bool> PlaceBidAsync(int lotId, int userId, decimal bidAmount);
    }
}