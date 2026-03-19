using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_Project.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }
    }
}
