using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<CategoryResponse>> GetAll()
    {
        var categories = _categoryService.GetAll();
        return Ok(categories);
    }

    [HttpPost]
    public ActionResult<CategoryResponse> Create([FromBody] CreateCategoryRequest request)
    {
        var response = _categoryService.Create(request);
        return CreatedAtAction(nameof(GetAll), new { }, response);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<CategoryResponse> Rename(Guid id, [FromBody] RenameCategoryRequest request)
    {
        var response = _categoryService.Rename(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _categoryService.Delete(id);
        return NoContent();
    }
}
