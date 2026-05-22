using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using BLL.Services;
using BLL.Exceptions;
using DAL.Interfaces;
using DAL.Entities;
using DAL.Services;

namespace Tests
{
    public class AuthServiceTests
    {
        private Mock<IConfiguration> GetMockConfiguration()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("very-long-secret-key-for-testing-purposes-only-12345");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestApi");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestClients");
            mockConfig.Setup(c => c["Jwt:ExpireDays"]).Returns("7");
            return mockConfig;
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();

            var password = "UserPass123!";
            var users = new List<User> { new User { Id = 1, Email = "test@example.com", PasswordHash = PasswordHasher.Hash(password), Role = UserRole.Registered } };
            mockUoW.Setup(u => u.UserRepository.GetAllAsync()).ReturnsAsync(users);

            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            var token = await authService.LoginAsync("test@example.com", password);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Equal(3, token.Split('.').Length);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();

            var users = new List<User> { new User { Id = 1, Email = "test@example.com", PasswordHash = PasswordHasher.Hash("CorrectPass123!") } };
            mockUoW.Setup(u => u.UserRepository.GetAllAsync()).ReturnsAsync(users);

            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync("test@example.com", "WrongPass123!"));
        }

        [Fact]
        public async Task RegisterAsync_ValidData_CreatesUserAndReturnsToken()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();

            var emptyUsers = new List<User>(); // База порожня, email унікальний
            mockUoW.Setup(u => u.UserRepository.GetAllAsync()).ReturnsAsync(emptyUsers);

            // Налаштовуємо мок, щоб при додаванні симулювати збереження в базі
            mockUoW.Setup(u => u.UserRepository.AddAsync(It.IsAny<User>()))
                   .Callback<User>(u => emptyUsers.Add(u))
                   .Returns(Task.CompletedTask);

            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            var token = await authService.RegisterAsync("newuser", "new@example.com", "Pass123!");

            Assert.False(string.IsNullOrWhiteSpace(token));
            mockUoW.Verify(u => u.UserRepository.AddAsync(It.Is<User>(user => user.Email == "new@example.com")), Times.Once);
            mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ThrowsUnauthorizedAccessException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();

            var users = new List<User> { new User { Email = "exist@example.com" } };
            mockUoW.Setup(u => u.UserRepository.GetAllAsync()).ReturnsAsync(users);
            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RegisterAsync("newuser", "exist@example.com", "Pass123!"));
            Assert.Equal("Користувач з таким email вже існує.", ex.Message);
        }

        [Theory]
        [InlineData("", "test@test.com", "pass")]
        [InlineData("user", "", "pass")]
        [InlineData("user", "test@test.com", "")]
        public async Task RegisterAsync_EmptyFields_ThrowsArgumentException(string username, string email, string password)
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();
            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => authService.RegisterAsync(username, email, password));
        }

        [Fact]
        public async Task GetCurrentUserAsync_ExistingUser_ReturnsDto()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();

            var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", Role = UserRole.Registered };
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(1)).ReturnsAsync(user);

            var authService = new AuthService(mockUoW.Object, mockConfig.Object);
            var result = await authService.GetCurrentUserAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetCurrentUserAsync_NonExistingUser_ThrowsEntityNotFoundException()
        {
            var mockUoW = new Mock<IUnitOfWork>();
            var mockConfig = GetMockConfiguration();
            mockUoW.Setup(u => u.UserRepository.GetByIdAsync(99)).ReturnsAsync((User?)null);

            var authService = new AuthService(mockUoW.Object, mockConfig.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => authService.GetCurrentUserAsync(99));
        }
    }
}