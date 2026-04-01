using ECommerce_Project.Api.DTOs.Category;
using ECommerce_Project.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_Project.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves a list of all product categories available in the catalog.
        /// </summary>
        /// <returns>An HTTP 200 OK response containing a list of category details.</returns>
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
        {
            return Ok(await _categoryService.GetAllAsync());
        }

        /// <summary>
        /// Retrieves the details of a specific category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category to retrieve.</param>
        /// <returns>An HTTP 200 OK response with the category details if found; 
        /// otherwise, an HTTP 404 Not Found response.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category is null ? NotFound() : Ok(category);
        }

        /// <summary>
        /// Creates a new product category in the catalog.
        /// </summary>
        /// <param name="dto">The data transfer object containing the necessary information to create a new category.</param>
        /// <returns>An HTTP 201 Created response containing the newly created category's details and its location URL.</returns>
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates the specified fields of an existing category.
        /// </summary>
        /// <param name="id">The unique identifier of the category to update.</param>
        /// <param name="dto">The data transfer object containing the updated fields. 
        /// Only the provided fields will be modified.</param>
        /// <returns>An HTTP 200 OK response with the updated category details if successful; 
        /// otherwise, an HTTP 404 Not Found response.</returns>
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<CategoryResponseDto>> Update(Guid id, UpdateCategoryDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        /// <summary>
        /// Deletes a specific category from the catalog.
        /// </summary>
        /// <param name="id">The unique identifier of the category to delete.</param>
        /// <returns>An HTTP 204 No Content response if the deletion is successful; 
        /// otherwise, an HTTP 404 Not Found response.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}