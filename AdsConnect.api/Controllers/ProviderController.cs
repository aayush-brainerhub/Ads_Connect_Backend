using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _providerService;

        public ProviderController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpGet("GetProviders")]
        public async Task<ActionResult> GetProviders()
        {
            ResponseData<List<ProviderDto>> responseData = new ResponseData<List<ProviderDto>>();
            try
            {
                var list = await _providerService.GetAllProviderService();
                if (list != null)
                {
                    responseData.data = list;
                    responseData.errMessage = "list of provider";
                    responseData.success = true;
                    return Ok(responseData);
                }
                else
                {
                    responseData.data = null;
                    responseData.success = false;
                    responseData.errMessage = "No provider found";
                    return Ok(responseData);
                }
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpGet("GetProviderById")]
        public async Task<ActionResult> GetProviderById(Guid id)
        {
            ResponseData<ProviderDto> responseData = new ResponseData<ProviderDto>();
            try
            {
                var provider = await _providerService.GetProviderByIdService(id);
                if (provider != null)
                {
                    responseData.data = provider;
                    responseData.success = true;
                    responseData.errMessage = "Provider Found";
                    return Ok(responseData);
                }
                else
                {
                    responseData.success = false;
                    responseData.errMessage = "Provider does not exits";
                    return Ok(responseData);
                }
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
