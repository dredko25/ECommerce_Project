using AutoMapper;
using ECommerce_Project.Api.DTOs.Category;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Project.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ECommerceDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(ECommerceDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<CategoryEntity>(dto);
            category.Id = Guid.NewGuid();

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(category.Id)
                ?? throw new Exception("Category not found after create");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .ToListAsync();

            return _mapper.Map<List<CategoryResponseDto>>(categories);
        }

        public async Task<CategoryResponseDto?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            return category is null ? null : _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null) return null;

            if (dto.Name != null) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;

            //_mapper.Map(dto, category);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
