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
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1, Status = LotStatus.Active, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(1), StartingPrice = 100 };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid>();

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);
            var result = await auctionService.PlaceBidAsync(1, 1, 150m);

            Assert.True(result);
            mockUoW.Verify(u => u.BidRepository.AddAsync(It.Is<Bid>(b => b.Amount == 150m)), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PlaceBidAsync_AmountTooLow_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1, Status = LotStatus.Active, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(1), StartingPrice = 100 };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid> { new Bid { LotId = 1, Amount = 150m } };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<AuctionValidationException>(() => auctionService.PlaceBidAsync(1, 1, 140m));
        }

        [Fact]
        public async Task PlaceBidAsync_AmountEqualToMaxBid_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1, Status = LotStatus.Active, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(1), StartingPrice = 100 };
            var user = new User { Id = 1, Role = UserRole.Registered };
            var existingBids = new List<Bid> { new Bid { LotId = 1, Amount = 200m } };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);
            mockUoW.Setup(u => u.BidRepository.GetAllAsync()).ReturnsAsync(existingBids);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<AuctionValidationException>(() => auctionService.PlaceBidAsync(1, 1, 200m));
        }

        [Fact]
        public async Task PlaceBidAsync_LotNotActive_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var pendingLot = new Lot { Id = 1, Status = LotStatus.Pending };
            var user = new User { Id = 1, Role = UserRole.Registered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(pendingLot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<AuctionValidationException>(() => auctionService.PlaceBidAsync(1, 1, 200m));
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionEnded_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var expiredLot = new Lot { Id = 1, Status = LotStatus.Active, StartTime = DateTime.UtcNow.AddDays(-5), EndTime = DateTime.UtcNow.AddDays(-1) };
            var user = new User { Id = 1, Role = UserRole.Registered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(expiredLot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<AuctionValidationException>(() => auctionService.PlaceBidAsync(1, 1, 200m));
        }

        [Fact]
        public async Task PlaceBidAsync_UnregisteredUser_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1 };
            var unregisteredUser = new User { Id = 2, Role = UserRole.Unregistered };

            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(2)).ReturnsAsync(unregisteredUser);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<AuctionValidationException>(() => auctionService.PlaceBidAsync(1, 2, 200m));
        }

        [Fact]
        public async Task PlaceBidAsync_LotNotFound_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(99)).ReturnsAsync((Lot?)null);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => auctionService.PlaceBidAsync(99, 1, 200m));
        }

        [Fact]
        public async Task PlaceBidAsync_UserNotFound_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1 };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(99)).ReturnsAsync((User?)null);

            var auctionService = new AuctionService(mockUoW.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => auctionService.PlaceBidAsync(1, 99, 200m));
        }
    }
}