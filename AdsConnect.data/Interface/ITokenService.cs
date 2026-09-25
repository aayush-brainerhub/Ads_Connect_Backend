using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface ITokenService
    {
        string GenerateToken(AuthUserDto user);
    }
}
