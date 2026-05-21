using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using BLL;
using BLL.Services;
using BLL.Exceptions;
using BLL.DTOs;
using DAL.Interfaces;
using DAL.Entities;

namespace Tests
{
    public class LotServiceTests
    {
        private readonly IMapper _mapper;

        public LotServiceTests()
        {
            // Використовуємо реальний конфіг AutoMapper для точного тестування DTO -> Entity
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task CreateLotAsync_ValidData_ReturnsLotIdAndSavesToDb()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);

            var lotDto = new LotCreateDto
            {
                Title = "Test Lot",
                Description = "Test Desc",
                StartingPrice = 100m,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(5),
                CategoryId = 1,
                SellerId = 1
            };

            mockUoW.Setup(u => u.LotRepository.AddAsync(It.IsAny<Lot>())).Returns(Task.CompletedTask);

            // Act
            var result = await lotService.CreateLotAsync(lotDto);

            // Assert
            mockUoW.Verify(u => u.LotRepository.AddAsync(It.IsAny<Lot>()), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateLotAsync_InvalidPrice_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);

            var lotDto = new LotCreateDto
            {
                StartingPrice = -10m, // Невалідна ціна
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                lotService.CreateLotAsync(lotDto));

            Assert.Equal("Стартова ціна має бути більшою за нуль.", exception.Message);
            mockUoW.Verify(u => u.LotRepository.AddAsync(It.IsAny<Lot>()), Times.Never);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateLotAsync_InvalidDates_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);

            var lotDto = new LotCreateDto
            {
                StartingPrice = 100m,
                StartTime = DateTime.UtcNow.AddDays(2),
                EndTime = DateTime.UtcNow.AddDays(1) // Дата завершення раніше за дату початку
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                lotService.CreateLotAsync(lotDto));

            Assert.Equal("Час завершення має бути пізнішим за час початку.", exception.Message);
        }

        [Fact]
        public async Task ApproveLotAsync_LotIsPending_ChangesStatusToActive()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var pendingLot = new Lot { Id = 1, Status = LotStatus.Pending };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(pendingLot);

            var lotService = new LotService(mockUoW.Object, _mapper);

            // Act
            await lotService.ApproveLotAsync(1, 99); // 99 - Id менеджера

            // Assert
            Assert.Equal(LotStatus.Active, pendingLot.Status);
            Assert.Equal(99, pendingLot.ApprovedByManagerId);
            mockUoW.Verify(u => u.LotRepository.Update(pendingLot), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveLotAsync_LotNotPending_ThrowsAuctionValidationException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var activeLot = new Lot { Id = 1, Status = LotStatus.Active }; // Вже активний
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(activeLot);

            var lotService = new LotService(mockUoW.Object, _mapper);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuctionValidationException>(() =>
                lotService.ApproveLotAsync(1, 99));

            Assert.Equal("Можна підтвердити лише лоти зі статусом Pending.", exception.Message);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}