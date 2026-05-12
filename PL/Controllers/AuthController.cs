using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BLL.Interfaces;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Якщо логін/пароль правильні, сервіс поверне токен
            var token = await _authService.LoginAsync(request.Email, request.Password);

            // Повертаємо токен користувачу
            return Ok(new { Token = token });
        }
    }
}