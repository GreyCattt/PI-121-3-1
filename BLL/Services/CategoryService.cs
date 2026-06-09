using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
        }

        public async Task<int> CreateCategoryAsync(string name)
        {
            var existing = await _unitOfWork.CategoryRepository.GetAsQueryable()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
            
            if (existing != null)
                throw new AuctionValidationException($"Категорія з назвою '{name}' вже існує.");

            var category = new Category { Name = name };
            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.Id;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null) throw new EntityNotFoundException("Category", id);

            var hasLots = await _unitOfWork.LotRepository.GetAsQueryable().AnyAsync(l => l.CategoryId == id);
            if (hasLots) throw new AuctionValidationException("Неможливо видалити категорію, оскільки вона містить лоти.");

            _unitOfWork.CategoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}