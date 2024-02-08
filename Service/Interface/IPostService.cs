using Microsoft.AspNetCore.Mvc;
using notion_clone.Dto.Category;

namespace notion_clone.Service.Interface;

public interface IPostService
{
    Task<List<PostResponseDto>> GetAllPosts(string userid);
    Task<PostResponseDto> GetSelectedPost(string userid, Guid id);
    Task<PostResponseDto> Create(string userId, PostCreateyDto dto);
    Task<PostResponseDto> Update(string userId, PostUpdateDto dto);
    Task DeleteById(string userId, Guid id);
}