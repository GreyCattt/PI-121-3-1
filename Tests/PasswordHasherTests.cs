using Xunit;
using DAL.Services;

namespace Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_GeneratesValidFormat()
        {
            // Arrange
            string password = "MySuperSecretPassword123!";

            // Act
            string hash = PasswordHasher.Hash(password);

            // Assert
            Assert.NotNull(hash);
            Assert.StartsWith("PBKDF2$", hash);
            Assert.Equal(4, hash.Split('$').Length);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "ValidPassword2026!";
            string hash = PasswordHasher.Hash(password);

            // Act
            bool isValid = PasswordHasher.Verify(password, hash);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void Verify_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            string correctPassword = "ValidPassword2026!";
            string wrongPassword = "WrongPassword!";
            string hash = PasswordHasher.Hash(correctPassword);

            // Act
            bool isValid = PasswordHasher.Verify(wrongPassword, hash);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Verify_LegacyPlaintextPassword_ReturnsTrue()
        {
            // Arrange
            string password = "PlaintextPassword123";
            // Емуляція старих даних, де хеш = просто пароль
            string storedHash = "PlaintextPassword123";

            // Act
            bool isValid = PasswordHasher.Verify(password, storedHash);

            // Assert
            Assert.True(isValid);
        }
    }
}