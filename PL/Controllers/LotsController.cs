using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.DTOs;
using PL.Models;

namespace PL.Controllers
{
    // Вказуємо, що це API контролер, і базовий шлях до нього буде /api/lots
    [ApiController]
    [Route("api/[controller]")]
    public class LotsController : ControllerBase
    {
        private readonly ILotService _lotService;

        // Dependency Injection: отримуємо наш сервіс із BLL
        public LotsController(ILotService lotService)
        {
            _lotService = lotService;
        }

        // GET: api/lots
        // Отримати всі лоти
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LotDto>>> GetAllLots()
        {
            var lots = await _lotService.GetAllLotsAsync();
            return Ok(lots); // Повертає HTTP статус 200 (OK) з даними
        }

        // GET: api/lots/5
        // Отримати лот за його ID
        [HttpGet("{id}")]
        public async Task<ActionResult<LotDto>> GetLotById(int id)
        {
            var lot = await _lotService.GetLotByIdAsync(id);
            return Ok(lot);
        }

        // POST: api/lots
        // Створити новий лот
        [HttpPost]
        public async Task<ActionResult<int>> CreateLot([FromBody] CreateLotRequest request)
        {
            // Ручний маппінг: перекладаємо дані з PL Model у BLL DTO
            var lotDto = new LotCreateDto
            {
                Title = request.Title,
                Description = request.Description,
                StartingPrice = request.StartingPrice,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CategoryId = request.CategoryId,
                SellerId = request.SellerId
            };

            var lotId = await _lotService.CreateLotAsync(lotDto);

            // Повертає статус 201 (Created) і посилання на метод GetLotById для перегляду нового лота
            return CreatedAtAction(nameof(GetLotById), new { id = lotId }, lotId);
        }

        // PUT: api/lots/5/approve?managerId=2
        // Підтвердити лот менеджером
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveLot(int id, [FromQuery] int managerId)
        {
            // Викликаємо логіку підтвердження
            await _lotService.ApproveLotAsync(id, managerId);

            // Повертає статус 204 (No Content) - успішно виконано, але тіло відповіді порожнє
            return NoContent();
        }
    }
}