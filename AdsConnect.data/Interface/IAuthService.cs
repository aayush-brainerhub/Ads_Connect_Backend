using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IAuthService
    {
        Task<(bool isValid, string message, LoginResultDto? result)> CheckUserService(SignInDto signInDto);

        Task<(bool created, string message, LoginResultDto? result)> RegisterService(RegisterDto registerDto);

        Task<(bool changed, string message)> ChangePasswordService(Guid userId, ChangePasswordDto changePasswordDto);

        Task<AuthUserDto?> GetUserByIdService(Guid userId);
    }
}
