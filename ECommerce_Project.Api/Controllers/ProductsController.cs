//using ECommerce_Project.Api.DTOs.Product;
//using ECommerce_Project.Api.Interfaces;
//using Microsoft.AspNetCore.Mvc;

//namespace ECommerce_Project.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ProductsController : ControllerBase
//    {
//        private readonly IProductService _productService;

//        public ProductsController(IProductService productService)
//        {
//            _productService = productService;
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProducts()
//        {
//            var products = await _productService.GetProductsAsync();
//            return Ok(products);
//        }
//    }
//}
using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<List<ProductSummaryDto>>> GetAll(
        [FromQuery] Guid? categoryId)
    {
        var products = categoryId.HasValue
            ? await _productService.GetByCategoryAsync(categoryId.Value)
            : await _productService.GetAllAsync();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, UpdateProductDto dto)
    {
        var updated = await _productService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}