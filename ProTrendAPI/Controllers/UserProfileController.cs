using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileService _profileService;
        public UserProfileController(UserProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<UserProfile>> GetProfile(string id)
        {
            var profile = await _profileService.GetUserProfileAsync(id);
            if (profile == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            return Ok(profile);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<UserProfile>> UpdateProfile(string id, [FromBody] UserProfile profile)
        {
            var result = await _profileService.UpdateProfile(id, profile);
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            return Ok(result);
        }
    }
}
