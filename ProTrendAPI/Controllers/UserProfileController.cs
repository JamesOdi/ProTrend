using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return Ok(await _profileService.GetUserProfileAsync(id));
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<UserProfile>> UpdateProfile(string id, [FromBody] UserProfile profile)
        {
            return Ok(await _profileService.UpdateProfile(id, profile));
        }
    }
}
