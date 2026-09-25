using AdsConnect.api.Utils;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ApiControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ProfileImageStore _imageStore;

        public ProfileController(IProfileService profileService, ProfileImageStore imageStore)
        {
            _profileService = profileService;
            _imageStore = imageStore;
        }

        [HttpGet("GetMyProfile")]
        public Task<ActionResult> GetMyProfile() =>
            ForCurrentUser<MyProfileDto>(async userId =>
                Success(await _profileService.GetMyProfileService(userId), "my profile"));

        [HttpPost("SaveAdvertiserProfile")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> SaveAdvertiserProfile(SaveAdvertiserProfileDto dto) =>
            ForCurrentUser<AdvertiserProfileDto>(async userId =>
            {
                var (saved, message, profile) = await _profileService.SaveAdvertiserProfileService(userId, dto);
                return saved ? Success(profile!, message) : Failure<AdvertiserProfileDto>(message);
            });

        [HttpPost("SaveProviderProfile")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> SaveProviderProfile(SaveProviderProfileDto dto) =>
            ForCurrentUser<ProviderProfileDto>(async userId =>
            {
                var (saved, message, profile) = await _profileService.SaveProviderProfileService(userId, dto);
                return saved ? Success(profile!, message) : Failure<ProviderProfileDto>(message);
            });

        [HttpPost("UploadProfileImage")]
        [RequestSizeLimit(ProfileImageStore.MaxBytes + 4096)]
        public Task<ActionResult> UploadProfileImage(IFormFile file) =>
            ForCurrentUser<string>(async userId =>
            {
                if (file == null || file.Length == 0)
                {
                    return Failure<string>("Choose an image to upload.");
                }

                if (file.Length > ProfileImageStore.MaxBytes)
                {
                    return Failure<string>("That image is larger than 2 MB.");
                }

                if (!ProfileImageStore.IsAllowed(file.ContentType))
                {
                    return Failure<string>($"That file type is not supported. Use {ProfileImageStore.AllowedTypesHint}.");
                }

                var url = await _imageStore.SaveAsync(file, HttpContext.RequestAborted);
                var (saved, message, replaced) = await _profileService.SetProfileImageService(userId, url);

                if (!saved)
                {
                    _imageStore.Delete(url);
                    return Failure<string>(message);
                }

                _imageStore.Delete(replaced);
                return Success(url, message);
            });

        [HttpDelete("RemoveProfileImage")]
        public Task<ActionResult> RemoveProfileImage() =>
            ForCurrentUser<string>(async userId =>
            {
                var (removed, message, replaced) = await _profileService.RemoveProfileImageService(userId);
                if (!removed)
                {
                    return Failure<string>(message);
                }

                _imageStore.Delete(replaced);
                return Success(message, message);
            });
    }
}
