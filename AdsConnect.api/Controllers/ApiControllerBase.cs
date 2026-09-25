using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AdsConnect.api.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Guid? CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var userId) ? userId : null;
        }

        protected async Task<ActionResult> ForCurrentUser<T>(
            Func<Guid, Task<ResponseData<T>>> action)
        {
            ResponseData<T> responseData = new ResponseData<T>();
            try
            {
                var userId = CurrentUserId();
                if (userId == null)
                {
                    responseData.success = false;
                    responseData.errMessage = "Account not found";
                    return Ok(responseData);
                }

                return Ok(await action(userId.Value));
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        protected static ResponseData<T> Success<T>(T data, string message) => new()
        {
            data = data,
            success = true,
            errMessage = message,
        };

        protected static ResponseData<T> Failure<T>(string message) => new()
        {
            data = default,
            success = false,
            errMessage = message,
        };
    }
}
