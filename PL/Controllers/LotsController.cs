using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // ДОДАНО ДЛЯ АВТОРИЗАЦІЇ
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Interfaces;
using BLL.DTOs;
using PL.Models;
using DAL.Entities;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotsController : ControllerBase
    {
        private readonly ILotService _lotService;

        public LotsController(ILotService lotService)
        {
            _lotService = lotService;
        }

        // GET: api/lots
        // Доступно всім (без атрибута [Authorize])
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LotDto>>> GetAllLots()
        {
            var lots = await _lotService.GetAllLotsAsync();
            return Ok(lots);
        }

        // GET: api/lots/search?searchQuery=iPhone&minPrice=100&maxPrice=500&categoryId=1&status=Active
        // Пошук і фільтрація лотів
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LotDto>>> SearchAndFilterLots(
            [FromQuery] string? searchQuery = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            // Конвертуємо рядок статусу в enum (якщо передано)
            LotStatus? lotStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (System.Enum.TryParse<LotStatus>(status, ignoreCase: true, out var parsedStatus))
                {
                    lotStatus = parsedStatus;
                }
                else
                {
                    // Якщо статус невалідний, повертаємо помилку
                    return BadRequest(new { message = $"Невалідний статус '{status}'. Допустимі значення: Pending, Active, Cancelled, Sold, NotSold" });
                }
            }

            var lots = await _lotService.SearchAndFilterLotsAsync(
                searchQuery: searchQuery,
                categoryId: categoryId,
                status: lotStatus,
                minPrice: minPrice,
                maxPrice: maxPrice);

            return Ok(lots);
        }

        // GET: api/lots/5
        // Доступно всім
        [HttpGet("{id}")]
        public async Task<ActionResult<LotDto>> GetLotById(int id)
        {
            var lot = await _lotService.GetLotByIdAsync(id);
            return Ok(lot);
        }

        // POST: api/lots
        // Створювати лоти можуть тільки зареєстровані, менеджери та адміни
        [HttpPost]
        [Authorize(Roles = "Registered,Manager,Admin")]
        public async Task<ActionResult<int>> CreateLot([FromBody] CreateLotRequest request)
        {
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
            return CreatedAtAction(nameof(GetLotById), new { id = lotId }, lotId);
        }

        // PUT: api/lots/5/approve?managerId=2
        // Підтверджувати лоти можуть ТІЛЬКИ менеджери або адміни
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveLot(int id, [FromQuery] int managerId)
        {
            await _lotService.ApproveLotAsync(id, managerId);
            return NoContent();
        }
    }
}