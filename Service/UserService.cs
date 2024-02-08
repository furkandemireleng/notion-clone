using notion_clone.Data;
using notion_clone.Data.Entity;
using notion_clone.Data.Entity.Model;
using notion_clone.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace notion_clone.Service;

public class UserService : IUserService
{
    private readonly NotionCloneDbContext dbContext;
    private readonly AppSettings setting;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly IMemoryCache memoryCache;

    public UserService(NotionCloneDbContext dbContext, 
        AppSettings setting,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMemoryCache memoryCache)
    {
        this.dbContext = dbContext;
        this.setting = setting;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.memoryCache = memoryCache;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<(string Token, DateTime ValidTo, string role)> GenerateToken(ApplicationUser user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.JWT!.Secret!));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var userRoles = await userManager.GetRolesAsync(user);

        if(userRoles == null || userRoles.Count == 0)
        {
            await EnsureRoleExistsAndAddUserToRoleAsync(user, "PERSONAL");
            userRoles = await userManager.GetRolesAsync(user);
        }

        var userRole = userRoles!.FirstOrDefault()!;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, userRole)
        };

        var tokeOptions = new JwtSecurityToken(
            issuer: setting!.JWT!.ValidIssuer,
            audience: setting!.JWT!.ValidAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: signinCredentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(tokeOptions), tokeOptions.ValidTo, userRole);
    }

    public async Task<RefreshTokenEntity> FindRefreshTokenAsync(string refreshToken)
    {
        var result = await dbContext.RefreshTokens
            .Include(f => f.User)
            .Where(x => x.Token == refreshToken)
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new Exception("U0002.");//The refresh token doesn't exist.
        }

        return result;
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string userId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);

        if(user == null)
        {
            throw new Exception("U0001");
        }

        var roleNames = await userManager.GetRolesAsync(user);
        var roleName = roleNames.FirstOrDefault();

        return new UserInfoResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            //WalletAddress=user.WalletAddress,
            Role = roleName,
            ///Language = user.Language
        };
    }



    public async Task<SigninResponseDto?> AddOrUpdateRefreshTokenAsync(ApplicationUser user)
    {
        var token = await GenerateToken(user);
        double unixExpireTime = (token.ValidTo.ToLocalTime() - DateTime.Now).TotalSeconds;

        var existingToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.User!.Id == user.Id);

        // If the token exists, update it, otherwise create a new one.
        var refreshToken = existingToken ?? new RefreshTokenEntity { User = user };

        UpdateOrSetRefreshTokenValues(refreshToken);

        if (existingToken == null)
        {
            await dbContext.RefreshTokens.AddAsync(refreshToken);
        }

        var result = await dbContext.SaveChangesAsync();

        return result > 0
            ? new SigninResponseDto
            {
                UserId = user.Id,
                AccessToken = token.Token,
                ExpireTime = unixExpireTime,
                RefreshToken = refreshToken.Token,
                Role = token.role
            }
            : null;
    }

    public async Task<SigninResponseDto?> UpdateRefreshTokenAsync(RefreshTokenEntity refreshToken)
    {
        var (newAccessToken, validTo, role) = await GenerateToken(refreshToken.User!);
        double unixExpireTime = (validTo.ToLocalTime() - DateTime.Now).TotalSeconds;

        UpdateOrSetRefreshTokenValues(refreshToken);

        dbContext.Entry(refreshToken).State = EntityState.Modified;

        var result = await dbContext.SaveChangesAsync();

        return result > 0
            ? new SigninResponseDto
            {
                UserId = refreshToken.User!.Id,
                AccessToken = newAccessToken,
                ExpireTime = unixExpireTime,
                RefreshToken = refreshToken.Token,
                Role = role,
                //Language = refreshToken.User.Language
            }
            : null;
    }

    private void UpdateOrSetRefreshTokenValues(RefreshTokenEntity refreshToken)
    {
        refreshToken.Token = GenerateRefreshToken();
        refreshToken.CreatedAt = DateTimeOffset.UtcNow;
        refreshToken.ExpireAt = DateTimeOffset.UtcNow.AddMinutes(10);
    }

    public async Task<int> DeleteRefreshToken(RefreshTokenEntity refreshToken)
    {
        dbContext.RefreshTokens.Remove(refreshToken);
        return await dbContext.SaveChangesAsync();
    }

    public async Task EnsureRoleExistsAndAddUserToRoleAsync(ApplicationUser newUser, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole(roleName));

            if (!roleResult.Succeeded)
            {
                throw new Exception("U0004");//The role can not be created
            }
        }

        var addUserToRoleResult = await userManager.AddToRoleAsync(newUser, roleName);

        if (!addUserToRoleResult.Succeeded)
        {
            throw new Exception("U0005");//The given role can not be set to the user.
        }
    }

    public async Task RemoveUserFromRolesAsync(ApplicationUser user)
    {
        var roleNames = userManager.GetRolesAsync(user!).Result;

        if(roleNames.Count() > 0)
        {
            await userManager.RemoveFromRolesAsync(user, roleNames);
        }
    }


    public ApplicationUserRole? GetUserRole(string userId)
    {
        var userData = dbContext!.Users
        .Include(u => u.UserRoles)
        .FirstOrDefault(u => u.Id == userId);

        var userRole = userData!.UserRoles!.FirstOrDefault();

        return userRole;
    }
}
