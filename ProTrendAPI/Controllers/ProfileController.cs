using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;
        private readonly IUserService _userService;
        private readonly NotificationService _notificationService;
        private readonly PostsService _postsService;
        public ProfileController(ProfileService profileService, PostsService postsService, NotificationService notificationService, IUserService userService)
        {
            _userService = userService;
            _profileService = profileService;
            _postsService = postsService;
            _notificationService = notificationService;
        }

        [HttpGet("get/id/{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
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
            var profile = _userService.GetProfile();
            var follow = await _profileService.Follow(profile, id);
            await _notificationService.FollowNotification(profile,id);
            return Ok(follow);
        }

        [HttpDelete("unfollow/{id}")]
        public async Task<ActionResult<BasicResponse>> UnFollow(Guid id)
        {
            return Ok(await _profileService.UnFollow(_userService.GetProfile(), id));
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

        [HttpGet("get/total")]
        public async Task<IActionResult> GetSupportTotal()
        {
            var profile = _userService.GetProfile();
            if (profile == null || profile.AccountType != Constants.Business)
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = "No support on non-business profiles" });
            }
            return Ok(new DataResponse { Data = await _postsService.GetTotalBalanceAsync(profile) });
        }
    }
}
