using Microsoft.AspNetCore.Mvc;
using notion_clone.Dto.Category;

namespace notion_clone.Service.Interface;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllCategory(string userid);
    Task<CategoryResponseDto> GetSelectedCategory(string userid, Guid id);
    Task<CategoryResponseDto> Update(string userId, CategoryUpdateDto dto);
    Task<CategoryResponseDto> Create(string userId, CreateCategoryDto dto);
    Task DeleteById(string userId, Guid id);
}