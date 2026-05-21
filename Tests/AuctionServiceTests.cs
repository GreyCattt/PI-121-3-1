using System;
using System.Collections.Generic;
using System.Linq;
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
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();

            var lot = new Lot
            {
                Id = 1,
                Status = LotStatus.Active,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(1),
                StartingPrice = 100
            };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid>();

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);

            // Act
            var result = await auctionService.PlaceBidAsync(1, 1, 150m);

            // Assert
            Assert.True(result);
            mockUoW.Verify(u => u.BidRepository.AddAsync(It.Is<Bid>(b => b.Amount == 150m && b.LotId == 1 && b.UserId == 1)), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PlaceBidAsync_AmountTooLow_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();

            var lot = new Lot
            {
                Id = 1,
                Status = LotStatus.Active,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(1),
                StartingPrice = 100
            };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid>
            {
                new Bid { Amount = 150m } // Поточна максимальна ставка 150
            };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                auctionService.PlaceBidAsync(1, 1, 140m)); // 140 менше за 150

            Assert.Contains("Сума ставки має бути більшою", exception.Message);
            mockUoW.Verify(u => u.BidRepository.AddAsync(It.IsAny<Bid>()), Times.Never);
        }

        [Fact]
        public async Task PlaceBidAsync_LotNotActive_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var pendingLot = new Lot { Id = 1, Status = LotStatus.Pending }; // Торги ще не почалися
            var user = new User { Id = 1, Role = UserRole.Registered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(pendingLot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);

            var auctionService = new AuctionService(mockUoW.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                auctionService.PlaceBidAsync(1, 1, 200m));

            Assert.Equal("Ставки приймаються лише на активні лоти.", exception.Message);
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionEnded_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var expiredLot = new Lot
            {
                Id = 1,
                Status = LotStatus.Active,
                StartTime = DateTime.UtcNow.AddDays(-5),
                EndTime = DateTime.UtcNow.AddDays(-1) // Аукціон завершився вчора
            };
            var user = new User { Id = 1, Role = UserRole.Registered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(expiredLot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);

            var auctionService = new AuctionService(mockUoW.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                auctionService.PlaceBidAsync(1, 1, 200m));

            Assert.Equal("Час проведення торгів для цього лота завершився або ще не почався.", exception.Message);
        }

        [Fact]
        public async Task PlaceBidAsync_UnregisteredUser_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1 };
            var unregisteredUser = new User { Id = 2, Role = UserRole.Unregistered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(2)).ReturnsAsync(unregisteredUser);

            var auctionService = new AuctionService(mockUoW.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                auctionService.PlaceBidAsync(1, 2, 200m));

            Assert.Equal("Тільки зареєстровані користувачі можуть робити ставки.", exception.Message);
        }
    }
}