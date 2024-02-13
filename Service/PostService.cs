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
            .Include(p => p.Categories) // Include the categories
            .Where(t => t.UserId == userid)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responseDtos = postEntities.Select(t => new PostResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Content = t.Content,
            CategoryEntities = t.Categories.Select(c => new CategoryEntity
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            }).ToList(),
            CreatedAt = t.CreatedAt
        }).ToList();

        return responseDtos;
    }
    public async Task<PostResponseDto> GetSelectedPost(string userid, Guid id)
    {
        var post = await dbContext.PostEntities
            .Include(p => p.Categories) // Include the categories
            .Where(t => t.Id == id && t.UserId == userid) // Filter by post id and user id
            .FirstOrDefaultAsync();

        if (post == null)
        {
            throw new NotFoundException(); // Post Not Found
        }

        var response = new PostResponseDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CategoryEntities = post.Categories.Select(c => new CategoryEntity
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            }).ToList(),
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
            UserId = userId
            //CreatedAt = DateTimeOffset.UtcNow // Assuming you want to set the creation time to the current time
        };

        post.Categories = new List<CategoryEntity>(); // Initialize the Categories collection

        // Check and associate existing categories or create new ones
        foreach (var categoryDto in dto.CategoryEntities)
        {
            if (categoryDto != null)
            {
                var existingCategory = await dbContext.CategoryEntities.FirstOrDefaultAsync(c => c.Id == categoryDto.Id);
                if (existingCategory != null)
                {
                    // Category already exists, associate with the post
                    post.Categories.Add(existingCategory);
                }
                else
                {
                    // Category does not exist, create a new one and associate with the post
                    var newCategory = new CategoryEntity
                    {
                        Id = categoryDto.Id,
                        Name = categoryDto.Name,
                        CreatedAt = categoryDto.CreatedAt
                    };
                    post.Categories.Add(newCategory);
                }
            }
        }

        // Save the entity to your database
        await dbContext.PostEntities.AddAsync(post);


        try
        {
            var result = await dbContext.SaveChangesAsync();

            if (result == 0)
            {
                throw new Exception("No entities were saved to the database.");
            }
        }
        catch (Exception ex)
        {
            // Log or capture the inner exception for details
            Console.WriteLine("An error occurred while saving the entity changes:");
            Console.WriteLine(ex.InnerException?.Message);
            throw; // Re-throw the exception to propagate it
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
            .Include(p => p.Categories) // Include categories to avoid lazy loading
            .Where(t => t.Id == dto.Id)
            .FirstOrDefaultAsync();

        if (findenPost == null)
        {
            throw new Exception(); // Or return any appropriate status code indicating resource not found
        }

        findenPost.Content = dto.Content;
        findenPost.Title = dto.Title;

        // Clear existing categories associated with the post
        findenPost.Categories.Clear();

        // Add or update categories based on DTO
        foreach (var categoryDto in dto.CategoryEntities)
        {
            var category = await dbContext.CategoryEntities.FindAsync(categoryDto.Id);
            if (category == null)
            {
                // Category not found, you might want to handle this case
                // For now, let's skip adding the category
                continue;
            }

            // Add the category to the post
            findenPost.Categories.Add(category);
        }

        try
        {
            dbContext.Update(findenPost);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log or handle the exception accordingly
            Console.WriteLine("An error occurred while saving the entity changes during update:");
            Console.WriteLine(ex.InnerException?.Message);
            throw; // Re-throw the exception to propagate it
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