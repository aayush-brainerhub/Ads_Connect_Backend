using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReferenceController : ControllerBase
    {
        private readonly IReferenceService _referenceService;

        public ReferenceController(IReferenceService referenceService)
        {
            _referenceService = referenceService;
        }

        [HttpGet("GetReferenceData")]
        [AllowAnonymous]
        public async Task<ActionResult> GetReferenceData()
        {
            ResponseData<ReferenceDataDto> responseData = new ResponseData<ReferenceDataDto>();
            try
            {
                var reference = await _referenceService.GetReferenceDataService();
                responseData.data = reference;
                responseData.success = true;
                responseData.errMessage = "reference data";
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message.ToString();
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpGet("GetLookupsBundle")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetLookupsBundle()
        {
            ResponseData<LookupsBundleDto> responseData = new ResponseData<LookupsBundleDto>();
            try
            {
                var bundle = await _referenceService.GetLookupsBundleService();
                responseData.data = bundle;
                responseData.success = true;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message;
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpPost("CreateLookupItem")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateLookupItem(CreateLookupItemDto dto)
        {
            ResponseData<string> responseData = new ResponseData<string>();
            try
            {
                var (success, message) = await _referenceService.CreateLookupItemService(dto);
                responseData.success = success;
                responseData.errMessage = message;
                responseData.data = message;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message;
                responseData.success = false;
                return Ok(responseData);
            }
        }

        [HttpPost("ToggleLookupStatus")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleLookupStatus(ToggleLookupStatusDto dto)
        {
            ResponseData<string> responseData = new ResponseData<string>();
            try
            {
                var (success, message) = await _referenceService.ToggleLookupStatusService(dto);
                responseData.success = success;
                responseData.errMessage = message;
                responseData.data = message;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                responseData.errMessage = ex.Message;
                responseData.success = false;
                return Ok(responseData);
            }
        }
    }
}
