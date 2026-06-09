using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAuctionService
    {
        Task<bool> PlaceBidAsync(int lotId, int userId, decimal bidAmount);
    }
}