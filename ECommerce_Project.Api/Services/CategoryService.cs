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
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ECommerceDbContext context, IMapper mapper, ILogger<CategoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new category using the specified data transfer object and returns the created category.
        /// </summary>
        /// <param name="dto">The data transfer object containing the information required to create a new category. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a data transfer object
        /// representing the newly created category.</returns>
        /// <exception cref="Exception">Thrown if the category cannot be retrieved after creation.</exception>
        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
        {
            _logger.LogInformation("Спроба створення нової категорії з назвою: {CategoryName}.", dto.Name);

            var category = _mapper.Map<CategoryEntity>(dto);
            category.Id = Guid.NewGuid();

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(category.Id)
                ?? throw new Exception("Category not found after create");
        }

        /// <summary>
        /// Asynchronously deletes the category with the specified identifier, if it exists.
        /// </summary>
        /// <param name="id">The unique identifier of the category to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the category
        /// was found and deleted; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null)
            {
                _logger.LogWarning("Категорію з ID {CategoryId} не знайдено, щоб видалити.", id);
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Категорію з ID {CategoryId} успішно видалено.", id);
            return true;
        }

        /// <summary>
        /// Asynchronously retrieves all categories along with their associated products.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of category data transfer
        /// objects, each including its related products. The list is empty if no categories are found.</returns>
        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .ToListAsync();

            return _mapper.Map<List<CategoryResponseDto>>(categories);
        }

        /// <summary>
        /// Asynchronously retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="CategoryResponseDto"/> representing the category if found; otherwise, <see langword="null"/>.</returns>
        public async Task<CategoryResponseDto?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            return _mapper.Map<CategoryResponseDto?>(category);
        }

        /// <summary>
        /// Updates the specified category with new values provided in the update data transfer object.
        /// </summary>
        /// <param name="id">The unique identifier of the category to update.</param>
        /// <param name="dto">An object containing the updated values for the category. Only non-null properties will be applied.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="CategoryResponseDto"/> with the updated category data if the category exists; otherwise, <see
        /// langword="null"/>.</returns>
        public async Task<CategoryResponseDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null) return null;

            if (dto.Name != null) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
