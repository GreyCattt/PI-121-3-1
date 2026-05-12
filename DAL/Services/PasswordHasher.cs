using System;
using System.Security.Cryptography;

namespace DAL.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            // Backward compatibility for older demo data that may still use plain text passwords.
            if (storedHash == password)
            {
                return true;
            }

            var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || parts[0] != "PBKDF2")
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedKey = Convert.FromBase64String(parts[3]);
                var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

                return CryptographicOperations.FixedTimeEquals(expectedKey, actualKey);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}