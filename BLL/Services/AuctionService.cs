using System;
using System.Linq;
using System.Threading.Tasks;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuctionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> PlaceBidAsync(int lotId, int userId, decimal bidAmount)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(lotId);
            if (lot == null)
                throw new EntityNotFoundException("Lot", lotId);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            if (user.Role == UserRole.Unregistered)
            {
                throw new AuctionValidationException("Тільки зареєстровані користувачі можуть робити ставки.");
            }

            if (lot.Status != LotStatus.Active)
            {
                throw new AuctionValidationException("Ставки приймаються лише на активні лоти.");
            }

            if (DateTime.UtcNow < lot.StartTime || DateTime.UtcNow > lot.EndTime)
            {
                throw new AuctionValidationException("Час проведення торгів для цього лота завершився або ще не почався.");
            }

            var allBids = await _unitOfWork.BidRepository.GetAllAsync();
            var lotBids = allBids.Where(b => b.LotId == lotId).ToList();

            decimal currentMaxPrice = lotBids.Any() ? lotBids.Max(b => b.Amount) : lot.StartingPrice;

            if (bidAmount <= currentMaxPrice)
            {
                throw new AuctionValidationException($"Сума ставки має бути більшою за поточну максимальну ціну ({currentMaxPrice}).");
            }

            var bid = new Bid
            {
                LotId = lotId,
                UserId = userId,
                Amount = bidAmount,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.BidRepository.AddAsync(bid);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}