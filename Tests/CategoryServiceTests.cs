using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MockQueryable.Moq;
using BLL.Services;
using BLL.Exceptions;
using DAL.Interfaces;
using DAL.Entities;

namespace Tests
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsDtoList()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cat1" },
                new Category { Id = 2, Name = "Cat2" }
            };
            mockUoW.Setup(u => u.CategoryRepository.GetAllAsync()).ReturnsAsync(categories);

            var categoryService = new CategoryService(mockUoW.Object);
            var result = await categoryService.GetAllCategoriesAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Name == "Cat1");
        }

        [Fact]
        public async Task CreateCategoryAsync_ValidName_ReturnsIdAndSaves()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var emptyCategories = new List<Category>().BuildMock();

            mockUoW.Setup(u => u.CategoryRepository.GetAsQueryable()).Returns(emptyCategories);
            var categoryService = new CategoryService(mockUoW.Object);

            var resultId = await categoryService.CreateCategoryAsync("New Category");

            mockUoW.Verify(u => u.CategoryRepository.AddAsync(It.Is<Category>(c => c.Name == "New Category")), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_DuplicateName_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var categories = new List<Category> { new Category { Id = 1, Name = "Electronics" } }.BuildMock();
            mockUoW.Setup(u => u.CategoryRepository.GetAsQueryable()).Returns(categories);

            var categoryService = new CategoryService(mockUoW.Object);

            var ex = await Assert.ThrowsAsync<AuctionValidationException>(() => categoryService.CreateCategoryAsync("electronics"));
            Assert.Contains("вже існує", ex.Message);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ValidCategory_DeletesSuccessfully()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var category = new Category { Id = 1, Name = "Empty Category" };
            var emptyLots = new List<Lot>().BuildMock();

            mockUoW.Setup(u => u.CategoryRepository.GetByIdAsync(1)).ReturnsAsync(category);
            mockUoW.Setup(u => u.LotRepository.GetAsQueryable()).Returns(emptyLots);

            var categoryService = new CategoryService(mockUoW.Object);
            await categoryService.DeleteCategoryAsync(1);

            mockUoW.Verify(u => u.CategoryRepository.Delete(category), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryHasLots_ThrowsAuctionValidationException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var category = new Category { Id = 1, Name = "Phones" };
            var lots = new List<Lot> { new Lot { Id = 1, CategoryId = 1 } }.BuildMock();

            mockUoW.Setup(u => u.CategoryRepository.GetByIdAsync(1)).ReturnsAsync(category);
            mockUoW.Setup(u => u.LotRepository.GetAsQueryable()).Returns(lots);

            var categoryService = new CategoryService(mockUoW.Object);

            var ex = await Assert.ThrowsAsync<AuctionValidationException>(() => categoryService.DeleteCategoryAsync(1));
            Assert.Equal("Неможливо видалити категорію, оскільки вона містить лоти.", ex.Message);
        }

        [Fact]
        public async Task DeleteCategoryAsync_NonExistingCategory_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.CategoryRepository.GetByIdAsync(99)).ReturnsAsync((Category?)null);

            var categoryService = new CategoryService(mockUoW.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => categoryService.DeleteCategoryAsync(99));
        }
    }
}