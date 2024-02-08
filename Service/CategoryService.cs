using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using notion_clone.Data;
using notion_clone.Data.Entity;
using notion_clone.Data.Entity.Model;
using notion_clone.Dto.Category;
using notion_clone.Service.Interface;
using SendGrid.Helpers.Errors.Model;

namespace notion_clone.Service;

public class CategoryService : ICategoryService
{
    private readonly NotionCloneDbContext dbContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppSettings setting;

    public CategoryService(
        NotionCloneDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        AppSettings setting
    )
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.setting = setting;
    }

    public async Task<List<CategoryResponseDto>> GetAllCategory(string userid)
    {
        var postEntities = await dbContext.CategoryEntities
            .OrderByDescending(t => t.CreatedAt) // Order by newest first
            .ToListAsync();

        var responseDtos = postEntities.Select(t => new CategoryResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();

        return responseDtos;
    }

    public async Task<CategoryResponseDto> GetSelectedCategory(string userid, Guid id)
    {
        var category = await dbContext.CategoryEntities
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };

        return response;
    }

    public async Task<CategoryResponseDto> Update(string userId, CategoryUpdateDto dto)
    {
        var category = await dbContext.CategoryEntities
            .Where(t => t.Id == dto.Id)
            .FirstOrDefaultAsync();

        category.Name = dto.Name;
        dbContext.Update(category);

        var result = await dbContext.SaveChangesAsync();

        if (result == 0)
        {
            throw new Exception();
        }

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };

        return response;
    }

    public async Task<CategoryResponseDto> Create(string userId, CreateCategoryDto dto)
    {
        var category = new CategoryEntity()
        {
            Name = dto.Name
        };
        // Save the entity to your database
        await dbContext.CategoryEntities.AddAsync(category);


        var result = await dbContext.SaveChangesAsync();

        if (result == 0)
        {
            throw new Exception();
        }

        var response = new CategoryResponseDto()
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };

        return response;
    }

    public async Task DeleteById(string userId, Guid id)
    {
        var category = await dbContext.CategoryEntities
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (category == null)
        {
            throw new Exception(); // Or return any appropriate status code indicating resource not found
        }

        dbContext.Remove(category);
        await dbContext.SaveChangesAsync();
    }
}