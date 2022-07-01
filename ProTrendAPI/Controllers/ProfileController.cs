using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models.User;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;
        private Profile? user;
        private readonly IUserService _userService;
        public ProfileController(ProfileService profileService, IUserService userService)
        {
            _userService = userService;
            _profileService = profileService;
        }

        [HttpGet("get/email/{email}")]
        public async Task<ActionResult<Profile?>> GetProfileByEmail(string email)
        {
            var profile = await _profileService.GetUserProfileByEmailAsync(email.ToLower());
            if (profile == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            return Ok(profile);
        }

        [HttpGet("get/id/{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var profile = await _profileService.GetUserProfileByIdAsync(id);
            if (profile == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            return Ok(profile);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<Profile>> UpdateProfile(Guid id, [FromBody] Profile profile)
        {
            var result = await _profileService.UpdateProfile(id, profile);
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            return Ok(result);
        }

        [HttpPost("follow/{id}")]
        public async Task<ActionResult<object>> Follow(Guid id)
        {
            user = _userService.GetUserProfile();
            return Ok(await _profileService.Follow(user.Identifier, id));
        }

        [HttpGet("get/followers/{id}")]
        public async Task<ActionResult<List<Followings>>> GetFollowers(Guid id)
        {
            return Ok(await _profileService.GetFollowersAsync(id));
        }

        [HttpGet("get/followings/{id}")]
        public async Task<ActionResult<List<Profile>>> GetFollowings(Guid id)
        {
            return Ok(await _profileService.GetFollowings(id));
        }

        [HttpGet("get/followers/{id}/count")]
        public async Task<ActionResult<string>> GetFollowerCount(Guid id)
        {
            return Ok(await _profileService.GetFollowerCount(id));
        }

        [HttpGet("get/followings/{id}/count")]
        public async Task<ActionResult<List<Profile>>> GetFollowingCount(Guid id)
        {
            return Ok(await _profileService.GetFollowingCount(id));
        }
    }
}
