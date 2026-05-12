using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.Interfaces;
using DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            // Отримуємо всіх користувачів
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            // Шукаємо користувача за Email та PasswordHash
            var user = users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Невірний email або пароль.");
            }

            // Формуємо дані для токена (Claims), включаючи роль
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Отримуємо налаштування з appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireDays = Convert.ToInt32(_configuration["Jwt:ExpireDays"]);

            // Створюємо токен
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            // Повертаємо токен у вигляді рядка
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}