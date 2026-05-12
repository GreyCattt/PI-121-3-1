using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // ДОДАНО ДЛЯ АВТОРИЗАЦІЇ
using System.Threading.Tasks;
using System.Security.Claims;
using BLL.Interfaces;
using PL.Models;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        // POST: api/auction/bids
        // Робити ставки можуть тільки зареєстровані користувачі та адміни
        [HttpPost("bids")]
        [Authorize(Roles = "Registered,Admin")]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest request)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { message = "Не вдалося визначити користувача з токена." });
            }

            await _auctionService.PlaceBidAsync(request.LotId, userId, request.Amount);
            return Ok(new { message = "Ставку успішно прийнято!" });
        }
    }
}