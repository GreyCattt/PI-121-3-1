using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MockQueryable.Moq;
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
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        // --- CREATE ---
        [Fact]
        public async Task CreateLotAsync_ValidData_ReturnsLotIdAndSavesToDb()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);
            var lotDto = new LotCreateDto { Title = "Test", StartingPrice = 100m, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(5) };

            mockUoW.Setup(u => u.LotRepository.AddAsync(It.IsAny<Lot>())).Returns(Task.CompletedTask);
            var result = await lotService.CreateLotAsync(lotDto);

            mockUoW.Verify(u => u.LotRepository.AddAsync(It.IsAny<Lot>()), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllLotsAsync_ReturnsMappedDtos()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lots = new List<Lot>
            {
                new Lot { Id = 1, Title = "Lot 1", Category = new Category(), Seller = new User() },
                new Lot { Id = 2, Title = "Lot 2", Category = new Category(), Seller = new User() }
            };

            mockUoW.Setup(u => u.LotRepository.GetAllAsync()).ReturnsAsync(lots);

            var lotService = new LotService(mockUoW.Object, _mapper);
            var result = await lotService.GetAllLotsAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, lot => lot.Title == "Lot 1");
        }

        [Fact]
        public async Task CreateLotAsync_InvalidPrice_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);
            var lotDto = new LotCreateDto { StartingPrice = -10m, StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(2) };

            var ex = await Assert.ThrowsAsync<AuctionValidationException>(() => lotService.CreateLotAsync(lotDto));
            Assert.Equal("Стартова ціна має бути більшою за нуль.", ex.Message);
        }

        [Fact]
        public async Task CreateLotAsync_InvalidDates_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lotService = new LotService(mockUoW.Object, _mapper);
            var lotDto = new LotCreateDto { StartingPrice = 100m, StartTime = DateTime.UtcNow.AddDays(2), EndTime = DateTime.UtcNow.AddDays(1) };

            await Assert.ThrowsAsync<AuctionValidationException>(() => lotService.CreateLotAsync(lotDto));
        }

        // --- GET & UPDATE & DELETE ---
        [Fact]
        public async Task GetLotByIdAsync_ExistingId_ReturnsMappedDto()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1, Title = "Title", Category = new Category(), Seller = new User() };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);

            var lotService = new LotService(mockUoW.Object, _mapper);
            var result = await lotService.GetLotByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Title", result.Title);
        }

        [Fact]
        public async Task GetLotByIdAsync_NonExistingId_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(99)).ReturnsAsync((Lot?)null);
            var lotService = new LotService(mockUoW.Object, _mapper);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => lotService.GetLotByIdAsync(99));
        }

        [Fact]
        public async Task UpdateLotAsync_ExistingId_UpdatesAndSaves()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1, Title = "Old Title" };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);

            var lotService = new LotService(mockUoW.Object, _mapper);
            await lotService.UpdateLotAsync(1, new LotUpdateDto { Title = "New Title", StartingPrice = 200m });

            Assert.Equal("New Title", lot.Title);
            Assert.Equal(200m, lot.StartingPrice);
            mockUoW.Verify(u => u.LotRepository.Update(lot), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteLotAsync_ExistingId_DeletesAndSaves()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lot = new Lot { Id = 1 };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(lot);

            var lotService = new LotService(mockUoW.Object, _mapper);
            await lotService.DeleteLotAsync(1);

            mockUoW.Verify(u => u.LotRepository.Delete(lot), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // --- APPROVE ---
        [Fact]
        public async Task ApproveLotAsync_LotIsPending_ChangesStatusToActive()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var pendingLot = new Lot { Id = 1, Status = LotStatus.Pending };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(pendingLot);

            var lotService = new LotService(mockUoW.Object, _mapper);
            await lotService.ApproveLotAsync(1, 99);

            Assert.Equal(LotStatus.Active, pendingLot.Status);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveLotAsync_LotNotPending_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var activeLot = new Lot { Id = 1, Status = LotStatus.Active };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(1)).ReturnsAsync(activeLot);

            var lotService = new LotService(mockUoW.Object, _mapper);

            await Assert.ThrowsAsync<AuctionValidationException>(() => lotService.ApproveLotAsync(1, 99));
        }

        [Fact]
        public async Task ApproveLotAsync_NonExistingId_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(99)).ReturnsAsync((Lot?)null);
            var lotService = new LotService(mockUoW.Object, _mapper);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => lotService.ApproveLotAsync(99, 1));
        }

        // --- SEARCH & FILTER ---
        [Fact]
        public async Task SearchAndFilterLotsAsync_NoFilters_ReturnsAllLots()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lots = new List<Lot>
            {
                new Lot { Id = 1, Category = new Category(), Seller = new User() },
                new Lot { Id = 2, Category = new Category(), Seller = new User() }
            }.BuildMock();

            mockUoW.Setup(u => u.LotRepository.GetAsQueryable()).Returns(lots);
            var lotService = new LotService(mockUoW.Object, _mapper);

            var result = await lotService.SearchAndFilterLotsAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAndFilterLotsAsync_WithFilters_ReturnsFilteredLots()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var lots = new List<Lot>
            {
                new Lot { Id = 1, Title = "iPhone 15", StartingPrice = 1000, CategoryId = 1, Status = LotStatus.Active, Category = new Category(), Seller = new User() },
                new Lot { Id = 2, Title = "Samsung", StartingPrice = 800, CategoryId = 2, Status = LotStatus.Active, Category = new Category(), Seller = new User() },
                new Lot { Id = 3, Title = "Old Phone", StartingPrice = 100, CategoryId = 1, Status = LotStatus.Sold, Category = new Category(), Seller = new User() }
            }.BuildMock();

            mockUoW.Setup(u => u.LotRepository.GetAsQueryable()).Returns(lots);
            var lotService = new LotService(mockUoW.Object, _mapper);

            // Шукаємо: містить "phone", категорія 1, ціна від 500 до 1200, статус Active
            var result = await lotService.SearchAndFilterLotsAsync("phone", 1, LotStatus.Active, 500m, 1200m);

            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("iPhone 15", resultList.First().Title);
        }
    }
}