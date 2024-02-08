using notion_clone.Data.Entity;
using notion_clone.Data.Entity.Model;
using notion_clone.Dto;

public interface IUserService
{
    Task<RefreshTokenEntity> FindRefreshTokenAsync(string refreshToken);
    Task<SigninResponseDto?> AddOrUpdateRefreshTokenAsync(ApplicationUser user);
    Task<SigninResponseDto?> UpdateRefreshTokenAsync(RefreshTokenEntity refreshToken);
    Task<int> DeleteRefreshToken(RefreshTokenEntity refreshToken);
    Task<UserInfoResponse> GetUserInfoAsync(string userId);
    //Task<UserInfoResponse> UpdateLanguage(string userId,UpdateLanguageDto dto);
    Task EnsureRoleExistsAndAddUserToRoleAsync(ApplicationUser newUser, string roleName);
    Task RemoveUserFromRolesAsync(ApplicationUser user);
    //Task<RoleLimitEntity?> GetRoleLimits(string roleId);
    ApplicationUserRole? GetUserRole(string userId);
}
