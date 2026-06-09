using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Services;
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
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Невірний email або пароль.");
            }

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing in configuration.");
            var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing in configuration.");
            var expireDaysText = _configuration["Jwt:ExpireDays"] ?? throw new InvalidOperationException("Jwt:ExpireDays is missing in configuration.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireDays = Convert.ToInt32(expireDaysText);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> RegisterAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username, email and password are required.");
            }

            var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Email == email))
            {
                throw new UnauthorizedAccessException("Користувач з таким email вже існує.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRole.Registered
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return await LoginAsync(email, password);
        }

        public async Task<AuthenticatedUserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new BLL.Exceptions.EntityNotFoundException("User", userId);
            }

            return new AuthenticatedUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}