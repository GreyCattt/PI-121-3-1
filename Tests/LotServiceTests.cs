using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using BLL.Services;
using BLL.Exceptions;
using DAL.Interfaces;
using DAL.Entities;

namespace Tests
{
    public class LotServiceTests
    {
        [Fact]
        public async Task ApproveLotAsync_LotIsPending_ChangesStatusToActive()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var mockMapper = new Mock<IMapper>(); // Маппер у цьому методі не використовується, але потрібен для конструктора

            var pendingLot = new Lot { Id = 5, Status = LotStatus.Pending };
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(5)).ReturnsAsync(pendingLot);

            var lotService = new LotService(mockUoW.Object, mockMapper.Object);

            // Act
            int managerId = 99;
            await lotService.ApproveLotAsync(5, managerId);

            // Assert
            Assert.Equal(LotStatus.Active, pendingLot.Status); // Статус має змінитися на Active
            Assert.Equal(managerId, pendingLot.ApprovedByManagerId); // ID менеджера має зберегтися

            // Перевіряємо, чи викликалось оновлення бази
            mockUoW.Verify(u => u.LotRepository.Update(pendingLot), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveLotAsync_LotNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            var mockUoW = new Mock<IUnitOfWork>();
            var mockMapper = new Mock<IMapper>();

            // База повертає null (лота не існує)
            mockUoW.Setup(u => u.LotRepository.GetByIdAsync(999)).ReturnsAsync((Lot)null);

            var lotService = new LotService(mockUoW.Object, mockMapper.Object);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                lotService.ApproveLotAsync(999, 1));
        }
    }
}