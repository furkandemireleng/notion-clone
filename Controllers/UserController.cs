using notion_clone.Data.Entity;
using notion_clone.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using notion_clone.Validatior.User;
using FluentValidation.Results;
using notion_clone.Constants;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using notion_clone.Dto;
using notion_clone.Extentions;
using notion_clone.Service.Interface;
using notion_clone.Service;

namespace notion_clone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IUserService userService;
        private readonly IEmailService emailService;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUserService userService,
            IEmailService emailService
        )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.userService = userService;
            this.emailService = emailService;
        }

        private async Task<SendGrid.Response> SendConfirmationEmail(ApplicationUser user)
        {
            var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailConfirmationTokenGeneratedAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);

            var callbackUrl = Url.Action("ConfirmEmail", "User",
                new { userId = user.Id, token = emailConfirmationToken }, protocol: HttpContext.Request.Scheme);

            var plainTextContent = $"Please confirm your account by clicking this link: {callbackUrl}";
            var htmlContent =
                $"<strong>Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a></strong>";
            var subject = "Email Confirmation";

            return await emailService.SendEmailAsync(user.Email!, subject, plainTextContent, htmlContent);
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailDto resendEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(resendEmailDto.Email!);

            if (user == null)
            {
                return BadRequest(new { error_code = 6, description = "User not found." });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { error_code = 4, description = "This user already exists." });
            }

            if (user.EmailConfirmationTokenGeneratedAt.HasValue &&
                DateTimeOffset.UtcNow.Subtract(user.EmailConfirmationTokenGeneratedAt.Value) < TimeSpan.FromMinutes(10))
            {
                return BadRequest(new
                {
                    error_code = 5,
                    description = "Please wait at least 10 minutes before requesting another confirmation email."
                });
            }

            var response = await SendConfirmationEmail(user);

            return StatusCode((int)response.StatusCode);
        }

        private async Task<IActionResult> HandleValidationErrorAsync(ValidationResult validationResult,
            RegisterDto register)
        {
            var error = validationResult.Errors.Select(err => err.ErrorMessage).FirstOrDefault();

            switch (error)
            {
                case ErrorMessages.InvalidEmail:
                    return BadRequest(new { error_code = 1, description = "Invalid Email" });
                case ErrorMessages.InvalidPassword:
                    return BadRequest(new { error_code = 2, description ="Invalid Password" });
                default:
                    // If the user exists but hasn't confirmed their email yet, tell them to check their inbox.
                    if (await userManager.FindByEmailAsync(register.Username!) != null)
                    {
                        return Ok(new { message = "Please check your email to confirm your registration." });
                    }

                    return BadRequest(validationResult.Errors);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            var validationResult = await new RegisterDtoValidator().ValidateAsync(register);

            if (!validationResult.IsValid)
            {
                return await HandleValidationErrorAsync(validationResult, register);
            }

            var newUser = new ApplicationUser
            {
                Email = register.Username,
                UserName = register.Username,
                Name = register.Name,
                IsActive = false
            };

            var createResult = await userManager.CreateAsync(newUser, register.Password!);

            if (!createResult.Succeeded)
            {
                return BadRequest(new { error_code = 3, description = "Username and email invalid." });
            }

            //await userService.EnsureRoleExistsAndAddUserToRoleAsync(newUser, "PERSONAL");


            //return StatusCode((int)(await SendConfirmationEmail(newUser)).StatusCode);
            return Ok();
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(UserUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await userManager.FindByIdAsync(dto.UserId!);

                if (user == null)
                    return BadRequest("Invalid user.");

                user.Name = dto.Name;
                // user.WalletAddress = dto.WalletAddress;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded) return Ok();

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest("Invalid user.");

            if (user.EmailConfirmationTokenGeneratedAt.HasValue &&
                DateTimeOffset.UtcNow.Subtract(user.EmailConfirmationTokenGeneratedAt.Value) > TimeSpan.FromMinutes(10))
            {
                return BadRequest("The confirmation link has expired. Please request a new one.");
            }

            var confirmResult = await userManager.ConfirmEmailAsync(user, token);

            if (!confirmResult.Succeeded)
                return BadRequest("Error confirming email.");

            user.IsActive = true;
            user.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return BadRequest("Error updating user information.");

            return Ok("Email confirmed successfully.");
        }


        [HttpPost("signin")]
        public async Task<ActionResult<SigninResponseDto>> Signin([FromBody] SigninDto signin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            var user = await userManager.FindByEmailAsync(signin.Username!);

            if (user == null)
            {
                return Unauthorized(new { error_code = 3, description = "Invalid username." });
            }

            if (!await userManager.CheckPasswordAsync(user, signin.Password!))
            {
                return Unauthorized(new { error_code = 3, description = "Invalid password." });
            }

            if (!user.IsActive)
            {
                return StatusCode(403, new { error_code = 4, description = "User has not been activated." });
            }

            var result = await userService.AddOrUpdateRefreshTokenAsync(user);

            if (result != null)
            {
                return Ok(result);
            }

            return BadRequest(new { message = "Error during the signin process." });
        }


        // [HttpPost("HandleGoogleLogin")]
        // public async Task<ActionResult<SigninResponseDto>> HandleGoogleLogin([FromBody] GoogleTokenDto tokenDto)
        // {
        //     if (string.IsNullOrEmpty(tokenDto?.Token))
        //     {
        //         return BadRequest(new { error_code = 1, description = "Token is missing." });
        //     }
        //
        //     var isValidToken = await googleAuthService.ValidateGoogleAccessToken(tokenDto.Token);
        //
        //     if (!isValidToken)
        //     {
        //         return BadRequest(new { error_code = 2, description = "Invalid Google token." });
        //     }
        //
        //     //var userInfo = await googleAuthService.GetUserProfileFromGoogle(tokenDto.Token);
        //
        //     if (userInfo == null)
        //     {
        //         return BadRequest(new { error_code = 3, description = "Invalid Google user info." });
        //     }
        //
        //     var email = userInfo.Email;
        //     var firstName = userInfo.GivenName;
        //     var lastName = userInfo.FamilyName;
        //     var id = userInfo.Id;
        //
        //     ApplicationUser? user = await userManager.FindByEmailAsync(email!);
        //
        //     if (user == null)
        //     {
        //         user = new ApplicationUser
        //         {
        //             Email = email,
        //             UserName = email,
        //             Name = $"{firstName} {lastName}",
        //             IsActive = true
        //         };
        //
        //         var createUserResult = await userManager.CreateAsync(user);
        //
        //         if (!createUserResult.Succeeded)
        //         {
        //             return BadRequest(new { error_code = 4, description = "Error creating user from external login." });
        //         }
        //     }
        //     else
        //     {
        //         if (string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        //         {
        //             user.Name = $"{firstName} {lastName}";
        //             await userManager.UpdateAsync(user);
        //         }
        //     }
        //
        //     // Link the external login with the user account, if not already linked
        //     var userLoginInfo = new UserLoginInfo("Google", id!, "Google");
        //
        //     if (!await googleAuthService.IsExternalLoginLinked(user, userLoginInfo))
        //     {
        //         var addLoginResult = await userManager.AddLoginAsync(user, userLoginInfo);
        //
        //         if (!addLoginResult.Succeeded)
        //         {
        //             return BadRequest(new { error_code = 5, description = "Error linking external login." });
        //         }
        //     }
        //
        //     var tokenResponse = await userService.AddOrUpdateRefreshTokenAsync(user);
        //
        //     if (tokenResponse == null)
        //     {
        //         return BadRequest(new { error_code = 6, description = "Error generating token." });
        //     }
        //
        //     return Ok(tokenResponse);
        // }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<SigninResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = User.GetUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "Failed to retrieve user's identity." });
                }

                var refreshTokenFromDb = await userService.FindRefreshTokenAsync(refreshToken.RefreshToken!);

                if (refreshTokenFromDb == null || refreshTokenFromDb.ExpireAt.AddMinutes(5) < DateTimeOffset.UtcNow)
                {
                    return Unauthorized();
                }

                if (refreshTokenFromDb.User!.Id != currentUserId)
                {
                    return Unauthorized(new { message = "Token does not belong to the authenticated user." });
                }

                var result = await userService.UpdateRefreshTokenAsync(refreshTokenFromDb);

                if (result != null)
                {
                    return Ok(result);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
        {
            try
            {
                var currentUserId = User.GetUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "Failed to retrieve user's identity." });
                }

                var result = await userService.GetUserInfoAsync(currentUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // [Authorize]
        // [HttpPatch("update-language")]
        // public async Task<ActionResult<UserInfoResponse>> UpdateLanguage(UpdateLanguageDto dto)
        // {
        //     try
        //     {
        //         var currentUserId = User.GetUserId();
        //
        //         if (string.IsNullOrEmpty(currentUserId))
        //         {
        //             return Unauthorized(new { message = "Failed to retrieve user's identity." });
        //         }
        //
        //         var result = await userService.UpdateLanguage(currentUserId, dto);
        //
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut([FromBody] SignoutDto signout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            var currentUserId = User.GetUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Failed to retrieve user's identity." });
            }

            var refreshTokenFromDb = await userService.FindRefreshTokenAsync(signout.RefreshToken!);

            if (refreshTokenFromDb == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            if (refreshTokenFromDb.User!.Id != currentUserId)
            {
                return Unauthorized(new { message = "Token does not belong to the authenticated user." });
            }

            var result = await userService.DeleteRefreshToken(refreshTokenFromDb);

            if (result > 0)
            {
                return Ok(new { message = "Signed out successfully." });
            }

            return BadRequest(new { message = "Error during the signout process." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.GetUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Failed to retrieve user's identity." });
            }

            var user = await userManager.FindByEmailAsync(changePassword.Username!);

            if (user == null)
            {
                return NotFound(new { error_code = 3, description = "Username invalid" });
            }

            if (user.Id != currentUserId)
            {
                return Unauthorized(new { message = "Token does not belong to the authenticated user." });
            }

            var result =
                await userManager.ChangePasswordAsync(user, changePassword.OldPassword!, changePassword.NewPassword!);

            if (!result.Succeeded)
            {
                return BadRequest(new { error_code = 2, description = "Invalid password" });
            }

            return Ok();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // if (!await googleAuthService.ValidateRecaptcha(forgotPassword.RecaptchaResponse!))
            // {
            //     return BadRequest(new { message = "reCAPTCHA validation failed." });
            // }

            var user = await userManager.FindByEmailAsync(forgotPassword.Email!);

            if (user == null)
            {
                return NotFound(new { error_code = 1, description = "Invalid email" });
            }

            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var plainTextContent = $"Your password reset token is: {passwordResetToken}";
            var htmlContent = $"<p>Your password reset token is: {passwordResetToken}</p>";
            var subject = "Password Reset";

            var result =
                await emailService.SendEmailAsync(forgotPassword.Email!, subject, plainTextContent, htmlContent);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(resetPassword.Email!);
            if (user == null)
            {
                return NotFound(new { error_code = 1, description = "Invalid email" });
            }

            var isTokenValid = await userManager.VerifyUserTokenAsync(user,
                userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword", resetPassword.Token!);

            if (!isTokenValid)
            {
                return BadRequest(new { error_code = 3, description = "Invalid or expired token" });
            }

            var result = await userManager.ResetPasswordAsync(user, resetPassword.Token!, resetPassword.NewPassword!);
            if (!result.Succeeded)
            {
                return BadRequest(new
                    { error_code = 2, description = "Invalid password", error = result, dto = resetPassword });
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("update-user-role")]
        public async Task<IActionResult> UpdateUserRole(string role)
        {
            var currentUserId = User.GetUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Failed to retrieve user's identity." });
            }

            var user = await userManager.FindByIdAsync(currentUserId);

            if (user == null)
            {
                return NotFound(new { error_code = 1, description = "Invalid user id" });
            }

            var userRole = userService.GetUserRole(currentUserId);

            if (userRole == null)
            {
                await userService.EnsureRoleExistsAndAddUserToRoleAsync(user, role);
            }
            else
            {
                var userRoleName = await roleManager.FindByIdAsync(userRole!.RoleId);

                if (userRoleName == null)
                {
                    return NotFound(new { error_code = 1, description = "User role detail not found." });
                }

                if (role.ToLowerInvariant() != userRoleName!.Name!.ToLowerInvariant())
                {
                    await userService.RemoveUserFromRolesAsync(user);
                    await userService.EnsureRoleExistsAndAddUserToRoleAsync(user, role);
                }
                else
                {
                    return BadRequest(new { error_code = 1, description = "The user already has the given role" });
                }
            }

            return Ok();
        }
    }
}