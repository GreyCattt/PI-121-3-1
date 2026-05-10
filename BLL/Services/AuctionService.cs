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
            // 1. Отримуємо лот та користувача з бази
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(lotId);
            if (lot == null)
                throw new EntityNotFoundException("Lot", lotId);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // 2. Перевірка ролі (робити ставки можуть тільки Registered, Manager або Admin)
            if (user.Role == UserRole.Unregistered)
            {
                throw new AuctionValidationException("Тільки зареєстровані користувачі можуть робити ставки.");
            }

            // 3. Перевірка статусу лота (торги йдуть тільки на Active)
            if (lot.Status != LotStatus.Active)
            {
                throw new AuctionValidationException("Ставки приймаються лише на активні лоти.");
            }

            // 4. Перевірка часу (чи не закінчився аукціон)
            if (DateTime.UtcNow < lot.StartTime || DateTime.UtcNow > lot.EndTime)
            {
                throw new AuctionValidationException("Час проведення торгів для цього лота завершився або ще не почався.");
            }

            // 5. Валідація суми ставки (має бути більша за поточну максимальну)
            var allBids = await _unitOfWork.BidRepository.GetAllAsync();
            var lotBids = allBids.Where(b => b.LotId == lotId).ToList();

            // Якщо ставки вже є - беремо найбільшу, якщо немає - стартову ціну
            decimal currentMaxPrice = lotBids.Any() ? lotBids.Max(b => b.Amount) : lot.StartingPrice;

            if (bidAmount <= currentMaxPrice)
            {
                throw new AuctionValidationException($"Сума ставки має бути більшою за поточну максимальну ціну ({currentMaxPrice}).");
            }

            // 6. Усі перевірки пройдені! Створюємо ставку
            var bid = new Bid
            {
                LotId = lotId,
                UserId = userId,
                Amount = bidAmount,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.BidRepository.AddAsync(bid);

            // Зберігаємо зміни в базу
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}