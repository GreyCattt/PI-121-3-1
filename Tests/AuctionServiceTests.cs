using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using BLL.Services;
using BLL.Exceptions;
using DAL.Interfaces;
using DAL.Entities;

namespace Tests
{
    public class AuctionServiceTests
    {
        [Fact]
        public async Task PlaceBidAsync_ValidData_ReturnsTrueAndSavesBid()
        {
            // ================== ARRANGE (Підготовка) ==================
            var mockUoW = new Mock<IUnitOfWork>();

            // Створюємо правильні тестові дані
            var lot = new Lot
            {
                Id = 1,
                Status = LotStatus.Active,
                StartTime = DateTime.UtcNow.AddDays(-1), // Почався вчора
                EndTime = DateTime.UtcNow.AddDays(1),    // Закінчиться завтра
                StartingPrice = 100
            };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid>(); // Ставок ще немає

            // Налаштовуємо "фейкову" базу даних так, щоб вона повертала наші об'єкти
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);

            // ================== ACT (Дія) ==================
            // Робимо ставку 150 (більше за стартову 100)
            var result = await auctionService.PlaceBidAsync(1, 1, 150m);

            // ================== ASSERT (Перевірка) ==================
            Assert.True(result); // Метод має повернути true
            // Перевіряємо, чи метод AddAsync був викликаний рівно 1 раз для збереження ставки
            mockUoW.Verify(u => u.BidRepository.AddAsync(It.IsAny<Bid>()), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PlaceBidAsync_UnregisteredUser_ThrowsAuctionValidationException()
        {
            // ================== ARRANGE (Підготовка) ==================
            var mockUoW = new Mock<IUnitOfWork>();

            var lot = new Lot { Id = 1 };
            // Користувач БЕЗ реєстрації
            var unregisteredUser = new User { Id = 2, Role = UserRole.Unregistered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(2)).ReturnsAsync(unregisteredUser);

            var auctionService = new AuctionService(mockUoW.Object);

            // ================== ACT & ASSERT ==================
            // Перевіряємо, що при спробі зробити ставку викинеться наша помилка валідації
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                auctionService.PlaceBidAsync(1, 2, 200m));

            Assert.Equal("Тільки зареєстровані користувачі можуть робити ставки.", exception.Message);
        }
    }
}