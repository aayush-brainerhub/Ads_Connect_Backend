using AdsConnect.api.Utils;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SignInController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public SignInController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(SignInDto signInDto)
        {
            ResponseData<LoginResultDto> responseData = new ResponseData<LoginResultDto>();
            try
            {
                var (isValid, message, result) = await _authService.CheckUserService(signInDto);
                if (isValid && result != null)
                {
                    responseData.data = result;
                    responseData.success = true;
                    responseData.errMessage = message;
                    return Ok(responseData);
                }

                responseData.success = false;
                responseData.errMessage = message;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            ResponseData<LoginResultDto> responseData = new ResponseData<LoginResultDto>();
            try
            {
                var (created, message, result) = await _authService.RegisterService(registerDto);
                if (created)
                {
                    if (result != null)
                    {
                        responseData.data = result;
                    }
                    // For Providers, result is null because they require approval. 
                    // We still return success = true.
                    responseData.success = true;
                    responseData.errMessage = message;
                    return Ok(responseData);
                }

                responseData.success = false;
                responseData.errMessage = message;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<ActionResult> Me()
        {
            ResponseData<AuthUserDto> responseData = new ResponseData<AuthUserDto>();
            try
            {
                var userId = CurrentUserId();
                var user = userId == null ? null : await _authService.GetUserByIdService(userId.Value);
                if (user != null)
                {
                    responseData.data = user;
                    responseData.success = true;
                    responseData.errMessage = "current user";
                    return Ok(responseData);
                }

                responseData.success = false;
                responseData.errMessage = "Account not found";
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            ResponseData<string> responseData = new ResponseData<string>();
            try
            {
                var userId = CurrentUserId();
                if (userId == null)
                {
                    responseData.success = false;
                    responseData.errMessage = "Account not found";
                    return Ok(responseData);
                }

                var (changed, message) = await _authService.ChangePasswordService(userId.Value, changePasswordDto);
                responseData.success = changed;
                responseData.errMessage = message;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

    }
}
