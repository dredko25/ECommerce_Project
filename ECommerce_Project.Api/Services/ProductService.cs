using AutoMapper;
using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Interfaces;
using ECommerce_Project.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Project.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly ECommerceDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(ECommerceDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductSummaryDto>> GetProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
        }
    }
}
