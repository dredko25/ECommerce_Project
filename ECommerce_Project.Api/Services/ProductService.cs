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

    public async Task<PagedResponse<ProductSummaryDto>> GetProductsAsync(ProductParams productParams)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (productParams.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == productParams.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(productParams.Search))
        {
            var searchLower = productParams.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower));
        }

        var totalItems = await query.CountAsync();

        var products = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

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