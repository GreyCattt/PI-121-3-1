using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.DTOs;
using PL.Models;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Авторизація користувача та отримання JWT.
        /// </summary>
        /// <example>
        /// { "email": "admin@auction.com", "password": "Admin123!" }
        /// </example>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Реєстрація нового користувача та отримання JWT.
        /// </summary>
        /// <example>
        /// { "username": "buyer1", "email": "buyer1@example.com", "password": "Buyer123!" }
        /// </example>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var token = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Повертає дані поточного користувача з JWT.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthenticatedUserDto>> Me()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { message = "Не вдалося визначити користувача з токена." });
            }

            var user = await _authService.GetCurrentUserAsync(userId);
            return Ok(user);
        }
    }
}