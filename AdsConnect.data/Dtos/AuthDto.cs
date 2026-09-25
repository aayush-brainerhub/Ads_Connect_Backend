using System.ComponentModel.DataAnnotations;

namespace AdsConnect.data.Dtos
{
    public class SignInDto
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string emailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string firstName { get; set; } = string.Empty;

        public string? lastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string emailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string role { get; set; } = string.Empty;

        public string? phoneNumber { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string currentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
        public string newPassword { get; set; } = string.Empty;
    }

    public class AuthUserDto
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string? lastName { get; set; }
        public string email { get; set; } = string.Empty;
        public string? phoneNumber { get; set; }
        public string? profileImageUrl { get; set; }
        public List<string> roles { get; set; } = new();
    }

    public class LoginResultDto
    {
        public bool requiresTwoFactor { get; set; } = false;
        public string token { get; set; } = string.Empty;
        public string? twoFactorToken { get; set; } = null;
    }
}
