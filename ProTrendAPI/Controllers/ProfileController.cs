using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class ProfileController : BaseController
    {
        public ProfileController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/id/{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            if (profile == null)
                return BadRequest(new BasicResponse { Message = Constants.UserNotFound });
            return Ok(profile);
        }

        [HttpPut("update")]
        public async Task<ActionResult<Profile>> UpdateProfile([FromBody] Profile updateProfile)
        {
            var result = await _profileService.UpdateProfile(_profile, updateProfile);
            if (result == null)
                return BadRequest(new BasicResponse { Message = "Update failed" });
            return Ok(result);
        }

        [HttpPost("follow/{id}")]
        public async Task<ActionResult<object>> Follow(Guid id)
        {
            if (id == _profile.Identifier)
                return BadRequest(new BasicResponse { Message = "Cannot follow one's self" });
            var followOk = await _profileService.Follow(_profile, id);
            if (!followOk)
                return BadRequest(new BasicResponse { Message = "Error following" });
            await _notificationService.FollowNotification(_profile, id);
            return Ok(new BasicResponse { Success = true, Message = "Follow successful" });
        }

        [HttpDelete("unfollow/{id}")]
        public async Task<ActionResult<BasicResponse>> UnFollow(Guid id)
        {
            var resultOk = await _profileService.UnFollow(_profile, id);
            if (resultOk)
                return Ok(new BasicResponse { Success = true, Message = "Unfollow successful" });
            return BadRequest(new BasicResponse { Message = "Unfollow failed" });
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
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowersAsync(id) });
        }

        [HttpGet("get/followings/{id}/count")]
        public async Task<ActionResult<List<Profile>>> GetFollowingCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowingCount(id) });
        }

        [HttpGet("get/gifts/total")]
        public async Task<IActionResult> GetGiftTotal()
        {
            if (_profile == null)
            {
                return BadRequest(new BasicResponse { Message = "Unauthorized" });
            }
            return Ok(new DataResponse { Data = await _postsService.GetTotalGiftsAsync(_profile.Identifier) });
        }
    }
}
