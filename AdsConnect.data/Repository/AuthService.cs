using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class AuthService : IAuthService
    {
        private const string SignInFailed = "Incorrect email address or password.";

        private readonly AdsConnectContext _adsConnectContext;
        private readonly ITokenService _tokenService;

        public AuthService(AdsConnectContext adsConnectContext, ITokenService tokenService)
        {
            _adsConnectContext = adsConnectContext;
            _tokenService = tokenService;
        }

        public async Task<(bool isValid, string message, LoginResultDto? result)> CheckUserService(SignInDto signInDto)
        {
            try
            {
                var email = (signInDto.emailAddress ?? string.Empty).Trim();
                if (email.Length == 0 || string.IsNullOrEmpty(signInDto.password))
                {
                    return (false, SignInFailed, null);
                }

                // AsTracking because LastLoginDate is written below; the context is
                // registered with NoTracking as its default query behaviour.
                var user = await _adsConnectContext.AppUsers
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    return (false, SignInFailed, null);
                }

                if (!BCrypt.Net.BCrypt.Verify(signInDto.password, user.PasswordHash))
                {
                    return (false, SignInFailed, null);
                }

                if (!user.IsActive)
                {
                    // Check if they are a Provider waiting for approval
                    var roles = await _adsConnectContext.UserRoles
                        .Include(ur => ur.Role)
                        .Where(ur => ur.UserId == user.UserId)
                        .ToListAsync();
                    
                    if (roles.Any(r => r.Role.RoleName == "Provider"))
                    {
                        return (false, "Your account is pending Admin approval.", null);
                    }
                    
                    return (false, "Your account has been deactivated.", null);
                }

                user.LastLoginDate = DateTime.UtcNow;
                await _adsConnectContext.SaveChangesAsync();

                var authUser = await ToAuthUser(user);
                var token = _tokenService.GenerateToken(authUser);
                var result = new LoginResultDto { token = token };
                return (true, "Logged in successfully", result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool created, string message, LoginResultDto? result)> RegisterService(RegisterDto registerDto)
        {
            try
            {
                var firstName = (registerDto.firstName ?? string.Empty).Trim();
                var email = (registerDto.emailAddress ?? string.Empty).Trim();
                var phoneNumber = string.IsNullOrWhiteSpace(registerDto.phoneNumber)
                    ? null
                    : registerDto.phoneNumber.Trim();

                if (firstName.Length == 0)
                {
                    return (false, "A first name is required.", null);
                }

                // CK_AppUser_Email enforces this shape in the database; checking it here
                // turns a constraint violation into a message the form can render.
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return (false, "Enter a valid email address.", null);
                }

                if ((registerDto.password ?? string.Empty).Length < 8)
                {
                    return (false, "The password must be at least 8 characters.", null);
                }

                var role = await _adsConnectContext.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == registerDto.role && r.IsActive);

                if (role == null)
                {
                    return (false, $"'{registerDto.role}' is not a valid role.", null);
                }

                // UX_AppUser_Email is unique on lower(Email), so the check and the
                // index agree on what counts as a duplicate.
                if (await _adsConnectContext.AppUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                {
                    return (false, "An account already exists for that email address.", null);
                }

                if (phoneNumber != null &&
                    await _adsConnectContext.AppUsers.AnyAsync(u => u.PhoneNumber == phoneNumber))
                {
                    return (false, "An account already exists for that phone number.", null);
                }

                // The id is generated here rather than by gen_random_uuid() so the
                // AppUser and its UserRole can be inserted in a single SaveChanges.
                var user = new AppUser
                {
                    UserId = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = string.IsNullOrWhiteSpace(registerDto.lastName) ? null : registerDto.lastName.Trim(),
                    Email = email,
                    PhoneNumber = phoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.password, BCrypt.Net.BCrypt.GenerateSalt()),
                    IsActive = role.RoleName != "Provider",
                    Preferences = "{}",
                };

                _adsConnectContext.AppUsers.Add(user);
                _adsConnectContext.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId,
                });

                await _adsConnectContext.SaveChangesAsync();

                if (role.RoleName == "Provider")
                {
                    // Force the database to false, bypassing EF Core's default value omission.
                    await _adsConnectContext.AppUsers
                        .Where(u => u.UserId == user.UserId)
                        .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false));
                }

                var authUserDto = new AuthUserDto
                {
                    userId = user.UserId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    profileImageUrl = user.ProfileImageUrl,
                    roles = new List<string> { role.RoleName },
                };

                if (role.RoleName == "Provider")
                {
                    return (true, "Our Team will get back to you once your account is approved.", null);
                }

                var token = _tokenService.GenerateToken(authUserDto);
                var result = new LoginResultDto { token = token };
                return (true, "Account created", result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool changed, string message)> ChangePasswordService(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                if ((changePasswordDto.newPassword ?? string.Empty).Length < 8)
                {
                    return (false, "The new password must be at least 8 characters.");
                }

                var user = await _adsConnectContext.AppUsers
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    return (false, "Account not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.currentPassword, user.PasswordHash))
                {
                    return (false, "The current password is incorrect.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.newPassword, BCrypt.Net.BCrypt.GenerateSalt());
                user.UpdatedDate = DateTime.UtcNow;
                await _adsConnectContext.SaveChangesAsync();

                return (true, "Password changed successfully");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AuthUserDto?> GetUserByIdService(Guid userId)
        {
            try
            {
                var user = await _adsConnectContext.AppUsers
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                return user == null ? null : await ToAuthUser(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<AuthUserDto> ToAuthUser(AppUser user) => new()
        {
            userId = user.UserId,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            profileImageUrl = user.ProfileImageUrl,
            roles = await _adsConnectContext.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.Role.IsActive)
                .OrderBy(ur => ur.Role.RoleName)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(),
        };
    }
}
