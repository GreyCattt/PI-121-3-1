using Xunit;
using DAL.Services;

namespace Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_GeneratesValidFormat()
        {
            string password = "MySuperSecretPassword123!";

            string hash = PasswordHasher.Hash(password);

            Assert.NotNull(hash);
            Assert.StartsWith("PBKDF2$", hash);
            Assert.Equal(4, hash.Split('$').Length);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            string password = "ValidPassword2026!";
            string hash = PasswordHasher.Hash(password);

            bool isValid = PasswordHasher.Verify(password, hash);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_IncorrectPassword_ReturnsFalse()
        {
            string correctPassword = "ValidPassword2026!";
            string wrongPassword = "WrongPassword!";
            string hash = PasswordHasher.Hash(correctPassword);

            bool isValid = PasswordHasher.Verify(wrongPassword, hash);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_LegacyPlaintextPassword_ReturnsTrue()
        {
            string password = "PlaintextPassword123";
            string storedHash = "PlaintextPassword123";

            bool isValid = PasswordHasher.Verify(password, storedHash);

            Assert.True(isValid);
        }
    }
}