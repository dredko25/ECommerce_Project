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
    private readonly ILogger<ProductService> _logger;

    public ProductService(ECommerceDbContext context, IMapper mapper, ILogger<ProductService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paged list of products that match the specified filtering and search criteria.
    /// </summary>
    /// <remarks>The method applies filtering by category and search term if provided. Results are paged
    /// according to the specified page number and page size. The returned items are mapped to summary DTOs and do not
    /// include full product details.</remarks>
    /// <param name="productParams">The parameters used to filter, search, and paginate the product results. Must specify valid page number and page
    /// size values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged response with product
    /// summary data matching the specified criteria. The response includes the total item count and paging information.</returns>
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
            .OrderBy(p => p.Name)
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

    /// <summary>
    /// Asynchronously retrieves a product by its unique identifier.
    /// </summary>
    /// <remarks>The returned product includes its associated category information. The operation does not
    /// track changes to the retrieved entity.</remarks>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ProductResponseDto"/>
    /// representing the product if found; otherwise, <see langword="null"/>.</returns>
    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : _mapper.Map<ProductResponseDto>(product);
    }

    /// <summary>
    /// Creates a new product using the specified data transfer object and returns the created product.
    /// </summary>
    /// <param name="dto">The data transfer object containing the information required to create a new product. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a data transfer object representing
    /// the newly created product.</returns>
    /// <exception cref="Exception">Thrown if the product cannot be retrieved after creation.</exception>
    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Спроба створення нового товару з назвою: {ProductName}.", dto.Name);

        var product = _mapper.Map<ProductEntity>(dto);
        product.Id = Guid.NewGuid();

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var createdProduct = await GetByIdAsync(product.Id);

        if (createdProduct == null)
        {
            _logger.LogError("Товар {ProductId} не знайдено в БД після збереження.", product.Id);
            throw new Exception("Product not found after create");
        }

        _logger.LogInformation("Товар {ProductName} успішно створено з ID: {ProductId}.", dto.Name, product.Id);
        return createdProduct;
    }

    /// <summary>
    /// Updates an existing product with the specified values and returns the updated product details.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="dto">An object containing the updated values for the product. Only non-null fields will be applied.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ProductResponseDto"/>
    /// with the updated product details, or <see langword="null"/> if the product does not exist.</returns>
    public async Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Не вдалося оновити, бо товар з ID {ProductId} не знайдено.", id);
            return null;
        }

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.QuantityAvailable.HasValue) product.QuantityAvailable = dto.QuantityAvailable.Value;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Товар з ID {ProductId} успішно оновлено.", id);
        return await GetByIdAsync(id);
    }

    /// <summary>
    /// Asynchronously deletes the product with the specified identifier from the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the product was
    /// found and deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Не вдалося видалити, бо товар з ID {ProductId} не знайдено.", id);
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Товар з ID {ProductId} успішно видалено.", id);
        return true;
    }
}