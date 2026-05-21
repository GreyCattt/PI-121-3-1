using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LotDto>>> GetAllLots()
        {
            var lots = await _lotService.GetAllLotsAsync();
            return Ok(lots);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LotDto>>> SearchAndFilterLots(
            [FromQuery] string? searchQuery = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            LotStatus? lotStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (System.Enum.TryParse<LotStatus>(status, ignoreCase: true, out var parsedStatus))
                {
                    lotStatus = parsedStatus;
                }
                else
                {
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

        [HttpGet("{id}")]
        public async Task<ActionResult<LotDto>> GetLotById(int id)
        {
            var lot = await _lotService.GetLotByIdAsync(id);
            return Ok(lot);
        }

        [HttpPost]
        [Authorize(Roles = "Registered,Manager,Admin")]
        public async Task<ActionResult<int>> CreateLot([FromBody] CreateLotRequest request)
        {
            var sellerId = GetCurrentUserId();
            if (sellerId == null)
            {
                return Unauthorized(new { message = "Не вдалося визначити користувача з токена." });
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var lotDto = new LotCreateDto
            {
                Title = request.Title,
                Description = request.Description,
                StartingPrice = request.StartingPrice,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CategoryId = request.CategoryId,
                SellerId = sellerId.Value,
                Status = ((userRole == "Admin" || userRole == "Manager") && request.Status.HasValue)
                ? request.Status.Value
                : LotStatus.Pending
            };

            var lotId = await _lotService.CreateLotAsync(lotDto);
            return CreatedAtAction(nameof(GetLotById), new { id = lotId }, lotId);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveLot(int id, [FromQuery] int managerId)
        {
            await _lotService.ApproveLotAsync(id, managerId);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateLot(int id, [FromBody] UpdateLotRequest request)
        {
            var lotDto = new LotUpdateDto
            {
                Title = request.Title,
                Description = request.Description,
                StartingPrice = request.StartingPrice,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CategoryId = request.CategoryId,
                Status = request.Status
            };

            await _lotService.UpdateLotAsync(id, lotDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteLot(int id)
        {
            await _lotService.DeleteLotAsync(id);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}