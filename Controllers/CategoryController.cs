using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using notion_clone.Attribute;
using notion_clone.Dto.Category;
using notion_clone.Service.Interface;

namespace notion_clone.Controllers;

[Route("api/{userId}/[controller]")]
[ApiController, Authorize]
[ValidateUserId]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService categoryService;


    public CategoryController(ICategoryService categoryService)
    {
        this.categoryService = categoryService;
    }

    [HttpGet("GetAllCategories")]
    public async Task<ActionResult<List<PostResponseDto>>> GetAllCategory(string userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await categoryService.GetAllCategory(userId);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetCategory")]
    public async Task<ActionResult<PostResponseDto>> GetSelectedCategory(string userId,
        Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await categoryService.GetSelectedCategory(userId, id);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddCategory")]
    public async Task<ActionResult<PostResponseDto>> Add(string userId,
        CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await categoryService.Create(userId, dto);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateCategory")]
    public async Task<ActionResult<PostResponseDto>> Update(string userId,
        CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await categoryService.Update(userId, dto);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteCategory")]
    public async Task<ActionResult> Delete(string userId,
        Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await categoryService.DeleteById(userId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}