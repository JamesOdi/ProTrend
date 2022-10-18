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
        public async Task<ActionResult<ActionResponse>> GetProfileById(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            if (profile == null)
                return NotFound(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.OK, Data = profile });
        }

        [HttpGet("mobile/get/id/{id}")]
        public async Task<ActionResult<ActionResponse>> MobileGetProfileById(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            if (profile == null)
                return NotFound(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.OK, Data = profile });
        }

        [HttpPut("update")]
        public async Task<ActionResult<ActionResponse>> UpdateProfile([FromBody] ProfileDTO updateProfile)
        {
            var profile = new Profile { AccountNumber = updateProfile.AccountNumber, BackgroundImageUrl = updateProfile.BackgroundImageUrl, FullName = updateProfile.FullName, UserName = updateProfile.UserName, PaymentPin = updateProfile.PaymentPin, Phone = updateProfile.Phone, ProfileImage = updateProfile.ProfileImage };
            var result = await _profileService.UpdateProfile(_profile, profile);
            if (result == null)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Update failed" });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.OK, Data = result });
        }

        [HttpPut("mobile/{id}/update")]
        public async Task<ActionResult<ActionResponse>> MobileUpdateProfile(string id, [FromBody] ProfileDTO updateProfile)
        {
            var profile = await _profileService.GetProfileByIdAsync(Guid.Parse(id));
            var update_profile = new Profile { AccountNumber = updateProfile.AccountNumber, BackgroundImageUrl = updateProfile.BackgroundImageUrl, FullName = updateProfile.FullName, UserName = updateProfile.UserName, PaymentPin = updateProfile.PaymentPin, Phone = updateProfile.Phone, ProfileImage = updateProfile.ProfileImage };
            var result = await _profileService.UpdateProfile(profile, update_profile);
            if (result == null)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Update failed" });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.OK, Data = result });
        }

        [HttpPost("follow/{id}")]
        public async Task<ActionResult<ActionResponse>> Follow(Guid id)
        {
            if (id == _profile.Identifier)
                return BadRequest(new ActionResponse { StatusCode = 403, Message = "Forbidden to follow yourself" });
            var followOk = await _profileService.Follow(_profile, id);
            if (!followOk)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Follow failed" });
            await _notificationService.FollowNotification(_profile, id);
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = "Follow successful" });
        }

        [HttpPost("mobile/follow/{from}/{to}")]
        public async Task<ActionResult<ActionResponse>> Follow(Guid from, Guid to)
        {
            if (from == to)
                return BadRequest(new ActionResponse { StatusCode = 403, Message = "Forbidden to follow yourself" });
            var profile = await _profileService.GetProfileByIdAsync(from);
            var followOk = await _profileService.Follow(profile, to);
            if (!followOk)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Follow failed" });
            await _notificationService.FollowNotification(profile, to);
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = "Follow successful" });
        }

        [HttpDelete("unfollow/{id}")]
        public async Task<ActionResult<ActionResponse>> UnFollow(Guid id)
        {
            var resultOk = await _profileService.UnFollow(_profile, id);
            if (!resultOk)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Unfollow failed" });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = "Unfollow successful" });
        }

        [HttpDelete("mobile/unfollow/{id}")]
        public async Task<ActionResult<ActionResponse>> UnFollow(Guid id, [FromBody] Profile profile)
        {
            var resultOk = await _profileService.UnFollow(profile, id);
            if (!resultOk)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Unfollow failed" });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = "Unfollow successful" });
        }

        [HttpGet("get/followers/{id}")]
        public async Task<ActionResult<ActionResponse>> GetFollowers(Guid id)
        {
            return Ok(new ActionResponse { Successful = false, StatusCode = 200, Message = ActionResponseMessage.OK, Data = await _profileService.GetFollowersAsync(id) });
        }

        [HttpGet("mobile/get/followers/{id}")]
        public async Task<ActionResult<ActionResponse>> MobileGetFollowers(Guid id)
        {
            return Ok(new ActionResponse { Successful = false, StatusCode = 200, Message = ActionResponseMessage.OK, Data = await _profileService.GetFollowersAsync(id) });
        }

        [HttpGet("get/followings/{id}")]
        public async Task<ActionResult<ActionResponse>> GetFollowings(Guid id)
        {
            return Ok(new ActionResponse { Successful = false, StatusCode = 200, Message = ActionResponseMessage.OK, Data = await _profileService.GetFollowings(id) });
        }

        [HttpGet("mobile/get/followings/{id}")]
        public async Task<ActionResult<ActionResponse>> MobileGetFollowings(Guid id)
        {
            return Ok(new ActionResponse { Successful = false, StatusCode = 200, Message = ActionResponseMessage.OK, Data = await _profileService.GetFollowings(id) });
        }

        [HttpGet("get/followers/{id}/count")]
        public async Task<ActionResult<ActionResponse>> GetFollowerCount(Guid id)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = Constants.OK, Data = await _profileService.GetFollowerCount(id) });
        }

        [HttpGet("mobile/get/followers/{id}/count")]
        public async Task<ActionResult<string>> MobileGetFollowerCount(Guid id)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = Constants.OK, Data = await _profileService.GetFollowerCount(id) });
        }

        [HttpGet("get/followings/{id}/count")]
        public async Task<ActionResult<List<ActionResponse>>> GetFollowingCount(Guid id)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = Constants.OK, Data = await _profileService.GetFollowingCount(id) });
        }

        [HttpGet("mobile/get/followings/{id}/count")]
        public async Task<ActionResult<List<ActionResponse>>> MobileGetFollowingCount(Guid id)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = Constants.OK, Data = await _profileService.GetFollowingCount(id) });
        }
    }
}
