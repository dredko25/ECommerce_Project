using ECommerce_Project.Api.DTOs.Product;
using ECommerce_Project.Api.Helpers;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Retrieves a paginated list of products 
    /// based on the provided filtering and search parameters.
    /// </summary>
    /// <param name="productParams">The parameters for filtering (e.g., category), searching by name, and pagination.</param>
    /// <returns>An HTTP 200 OK response containing a paginated list of product summaries.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductSummaryDto>>> GetProducts(
    [FromQuery] ProductParams productParams)
    {
        var products = await _productService.GetProductsAsync(productParams);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves the detailed information of a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>An HTTP 200 OK response with the product details if found; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Creates a new product in the catalog. Requires Administrator privileges.
    /// </summary>
    /// <param name="dto">The data transfer object containing the necessary 
    /// information to create a new product.</param>
    /// <returns>An HTTP 201 Created response containing 
    /// the newly created product's details and its location URL.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates the specified fields of an existing product. Requires Administrator privileges.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="dto">The data transfer object containing the updated fields. 
    /// Only provided fields will be modified.</param>
    /// <returns>An HTTP 200 OK response with the updated product details if successful; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, UpdateProductDto dto)
    {
        var updated = await _productService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Deletes a specific product from the catalog. Requires Administrator privileges.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>An HTTP 204 No Content response if the deletion is successful; 
    /// otherwise, an HTTP 404 Not Found response.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _productService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}