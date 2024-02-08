using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using notion_clone.Attribute;
using notion_clone.Dto.Category;
using notion_clone.Service.Interface;

namespace notion_clone.Controllers;

[Route("api/{userId}/[controller]")]
[ApiController, Authorize]
[ValidateUserId]
public class PostController : ControllerBase
{
    private readonly IPostService postService;


    public PostController(IPostService postService)
    {
        this.postService = postService;
    }

    [HttpGet("GetAllPosts")]
    public async Task<ActionResult<List<PostResponseDto>>> GetAllPosts(string userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await postService.GetAllPosts(userId);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetPost")]
    public async Task<ActionResult<PostResponseDto>> Get(string userId,
        Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await postService.GetSelectedPost(userId, id);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddPost")]
    public async Task<ActionResult<PostResponseDto>> Add(string userId,
        PostCreateyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await postService.Create(userId, dto);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdatePost")]
    public async Task<ActionResult<PostResponseDto>> Update(string userId,
        PostUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var filteredTokens = await postService.Update(userId, dto);

            return Ok(filteredTokens);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("DeletePost")]
    public async Task<ActionResult> Delete(string userId,
        Guid id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await postService.DeleteById(userId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}