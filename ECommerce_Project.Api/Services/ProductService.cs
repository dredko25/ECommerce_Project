//using AutoMapper;
//using ECommerce_Project.Api.DTOs.Product;
//using ECommerce_Project.Api.Interfaces;
//using ECommerce_Project.DataAccess;
//using Microsoft.EntityFrameworkCore;

//namespace ECommerce_Project.Api.Services
//{
//    public class ProductService : IProductService
//    {
//        private readonly ECommerceDbContext _context;
//        private readonly IMapper _mapper;

//        public ProductService(ECommerceDbContext context, IMapper mapper)
//        {
//            _context = context;
//            _mapper = mapper;
//        }

//        public async Task<IEnumerable<ProductSummaryDto>> GetProductsAsync()
//        {
//            var products = await _context.Products
//                .Include(p => p.Category)
//                .ToListAsync();

//            return _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
//        }
//    }
//}

using AutoMapper;
using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Helpers;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using ECommerce_Project.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    //public async Task<List<ProductSummaryDto>> GetAllAsync()
    //{
    //    var products = await _context.Products
    //        .AsNoTracking()
    //        .Include(p => p.Category)
    //        .ToListAsync();

    //    return _mapper.Map<List<ProductSummaryDto>>(products);
    //}

    //public async Task<List<ProductSummaryDto>> GetByCategoryAsync(Guid categoryId)
    //{
    //    var products = await _context.Products
    //        .AsNoTracking()
    //        .Include(p => p.Category)
    //        .Where(p => p.CategoryId == categoryId)
    //        .ToListAsync();

    //    return _mapper.Map<List<ProductSummaryDto>>(products);
    //}

    public async Task<PagedResponse<ProductSummaryDto>> GetProductsAsync(ProductParams productParams)
    {
        // 1. Починаємо формувати запит до бази
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        // 2. Фільтрація за категорією
        if (productParams.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == productParams.CategoryId.Value);
        }

        // 3. Пошук за назвою (без урахування регістру)
        if (!string.IsNullOrWhiteSpace(productParams.Search))
        {
            var searchLower = productParams.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower));
        }

        // 4. Рахуємо загальну кількість ТАКИХ товарів (для пагінації)
        var totalItems = await query.CountAsync();

        // 5. Застосовуємо пагінацію (Skip і Take)
        var products = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

        // 6. Формуємо відповідь
        return new PagedResponse<ProductSummaryDto>
        {
            Items = _mapper.Map<List<ProductSummaryDto>>(products),
            TotalCount = totalItems,
            PageNumber = productParams.PageNumber,
            PageSize = productParams.PageSize
        };
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<ProductEntity>(dto);
        product.Id = Guid.NewGuid();

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id)
            ?? throw new Exception("Product not found after create");
    }

    public async Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return null;

        _mapper.Map(dto, product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}