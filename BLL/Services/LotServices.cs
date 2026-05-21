using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class LotService : ILotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LotDto>> GetAllLotsAsync()
        {
            var lots = await _unitOfWork.LotRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<LotDto> GetLotByIdAsync(int id)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(id);
            if (lot == null)
                throw new EntityNotFoundException("Lot", id);

            return _mapper.Map<LotDto>(lot);
        }

        public async Task<int> CreateLotAsync(LotCreateDto lotDto)
        {
            // 1. Бізнес-валідація
            if (lotDto.StartingPrice <= 0)
                throw new AuctionValidationException("Стартова ціна має бути більшою за нуль.");

            if (lotDto.EndTime <= lotDto.StartTime)
                throw new AuctionValidationException("Час завершення має бути пізнішим за час початку.");

            // 2. Маппінг DTO в Entity (статус Pending встановиться автоматично через налаштування AutoMapper)
            var lot = _mapper.Map<Lot>(lotDto);

            // 3. Збереження в базу
            await _unitOfWork.LotRepository.AddAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return lot.Id;
        }

        public async Task ApproveLotAsync(int lotId, int managerId)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(lotId);
            if (lot == null)
                throw new EntityNotFoundException("Lot", lotId);

            // Перевірка бізнес-правила: підтвердити можна тільки лот, що очікує
            if (lot.Status != LotStatus.Pending)
                throw new AuctionValidationException("Можна підтвердити лише лоти зі статусом Pending.");

            lot.Status = LotStatus.Active;
            lot.ApprovedByManagerId = managerId;

            _unitOfWork.LotRepository.Update(lot);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<LotDto>> SearchAndFilterLotsAsync(
            string? searchQuery = null,
            int? categoryId = null,
            LotStatus? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            // Отримуємо IQueryable для динамічного побудування запиту
            var query = _unitOfWork.LotRepository.GetAsQueryable();

            // Завантажуємо асоційовані дані (Category, Seller, Bids) для коректного маппінгу
            query = query.Include(l => l.Category)
                         .Include(l => l.Seller)
                         .Include(l => l.Bids);

            // Застосуємо фільтри
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Пошук по назві та опису (case-insensitive)
                query = query.Where(l => l.Title.ToLower().Contains(searchQuery.ToLower()) ||
                                         l.Description.ToLower().Contains(searchQuery.ToLower()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(l => l.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(l => l.StartingPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.StartingPrice <= maxPrice.Value);
            }

            // Виконуємо запит і отримуємо результати з БД
            var lots = await query.ToListAsync();

            // Маппуємо в DTO
            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }
        public async Task DeleteLotAsync(int id)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(id);
            if (lot == null)
            throw new EntityNotFoundException("Lot", id);

            _unitOfWork.LotRepository.Delete(lot);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}