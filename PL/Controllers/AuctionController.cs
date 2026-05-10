using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BLL.Interfaces;
using PL.Models;

namespace PL.Controllers
{
    // Базовий шлях буде /api/auction
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        // Отримуємо сервіс аукціону з шару бізнес-логіки
        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        // POST: api/auction/bids
        // Зробити ставку на лот
        [HttpPost("bids")]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest request)
        {
            // Викликаємо метод з нашого AuctionService
            // Передаємо дані з PL Model (request) у параметри методу BLL
            await _auctionService.PlaceBidAsync(request.LotId, request.UserId, request.Amount);

            // Якщо ставка пройшла успішно і не викликала помилок, повертаємо статус 200 (OK)
            return Ok(new { message = "Ставку успішно прийнято!" });
        }
    }
}