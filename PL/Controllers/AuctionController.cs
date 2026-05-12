using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // ДОДАНО ДЛЯ АВТОРИЗАЦІЇ
using System.Threading.Tasks;
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
            await _auctionService.PlaceBidAsync(request.LotId, request.UserId, request.Amount);
            return Ok(new { message = "Ставку успішно прийнято!" });
        }
    }
}