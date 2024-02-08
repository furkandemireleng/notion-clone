using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using notion_clone.Data;
using notion_clone.Data.Entity;
using notion_clone.Data.Entity.Model;
using notion_clone.Dto.Category;
using notion_clone.Service.Interface;
using SendGrid.Helpers.Errors.Model;


namespace notion_clone.Service;

public class PostService : IPostService
{
    private readonly NotionCloneDbContext dbContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppSettings setting;

    public PostService(
        NotionCloneDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        AppSettings setting
    )
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.setting = setting;
    }


    public async Task<List<PostResponseDto>> GetAllPosts(string userid)
    {
        var postEntities = await dbContext.PostEntities
            .Where(t => t.UserId == userid)
            .OrderByDescending(t => t.CreatedAt) // Order by newest first
            .ToListAsync();

        var responseDtos = postEntities.Select(t => new PostResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Content = t.Content,
            CategoryEntities = (List<CategoryEntity>?)t.Categories,
            CreatedAt = t.CreatedAt
        }).ToList();

        return responseDtos;
    }

    public async Task<PostResponseDto> GetSelectedPost(string userid, Guid id)
    {
        var post = await dbContext.PostEntities
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (post == null)
        {
            throw new NotFoundException(); //Nft Not Found
        }

        var response = new PostResponseDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CategoryEntities = (List<CategoryEntity>?)post.Categories,
            CreatedAt = post.CreatedAt
        };

        return response;
    }

    public async Task<PostResponseDto> Create(string userId, PostCreateyDto dto)
    {
        var post = new PostEntity()
        {
            Title = dto.Title,
            Content = dto.Content,
            Categories = dto.CategoryEntities,
        };


        // Save the entity to your database
        await dbContext.PostEntities.AddAsync(post);


        var result = await dbContext.SaveChangesAsync();

        if (result == 0)
        {
            throw new Exception();
        }

        var response = new PostResponseDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CategoryEntities = (List<CategoryEntity>?)post.Categories,
            CreatedAt = post.CreatedAt
        };

        return response;
    }

    public async Task<PostResponseDto> Update(string userId, PostUpdateDto dto)
    {
        var findenPost = await dbContext.PostEntities
            .Where(t => t.Id == dto.Id)
            .FirstOrDefaultAsync();

        if (findenPost == null)
        {
            throw new Exception(); // Or return any appropriate status code indicating resource not found
        }

        findenPost.Categories = dto.CategoryEntities;
        findenPost.Content = dto.Content;
        findenPost.Title = dto.Title;

        dbContext.Update(findenPost);
        var result = await dbContext.SaveChangesAsync();

        if (result == 0)
        {
            throw new Exception();
        }

        var response = new PostResponseDto
        {
            Id = dto.Id,
            Title = findenPost.Title,
            Content = findenPost.Content,
            CategoryEntities = (List<CategoryEntity>?)findenPost.Categories,
            CreatedAt = findenPost.CreatedAt
        };

        return response;
    }

    public async Task DeleteById(string userId, Guid id)
    {
        var post = await dbContext.PostEntities
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (post == null)
        {
            throw new Exception(); // Or return any appropriate status code indicating resource not found
        }

        dbContext.Remove(post);
        await dbContext.SaveChangesAsync();
    }
}