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

        [HttpGet("mobile/get/id/{id}")]
        public async Task<ActionResult<Profile>> MobileGetProfileById(Guid id)
        {
            var profile = await _profileService.GetProfileByIdAsync(id);
            if (profile == null)
                return BadRequest(new BasicResponse { Message = Constants.UserNotFound });
            return Ok(profile);
        }

        [HttpPut("update")]
        public async Task<ActionResult<Profile>> UpdateProfile([FromBody] ProfileDTO updateProfile)
        {
            var profile = new Profile { AccountNumber = updateProfile.AccountNumber, BackgroundImageUrl = updateProfile.BackgroundImageUrl, FullName = updateProfile.FullName, UserName = updateProfile.UserName, PaymentPin = updateProfile.PaymentPin, Phone = updateProfile.Phone, ProfileImage = updateProfile.ProfileImage };
            var result = await _profileService.UpdateProfile(_profile, profile);
            if (result == null)
                return BadRequest(new BasicResponse { Message = "Update failed" });
            return Ok(result);
        }

        [HttpPut("mobile/{id}/update")]
        public async Task<ActionResult<Profile>> MobileUpdateProfile(string id, [FromBody] ProfileDTO updateProfile)
        {
            var profile = await _profileService.GetProfileByIdAsync(Guid.Parse(id));
            var update_profile = new Profile { AccountNumber = updateProfile.AccountNumber, BackgroundImageUrl = updateProfile.BackgroundImageUrl, FullName = updateProfile.FullName, UserName = updateProfile.UserName, PaymentPin = updateProfile.PaymentPin, Phone = updateProfile.Phone, ProfileImage = updateProfile.ProfileImage };
            var result = await _profileService.UpdateProfile(profile, update_profile);
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

        [HttpPost("mobile/follow/{from}/{to}")]
        public async Task<ActionResult<object>> Follow(Guid from, Guid to)
        {
            if (from == to)
                return BadRequest(new BasicResponse { Message = "Cannot follow one's self" });
            var profile = await _profileService.GetProfileByIdAsync(from);
            var followOk = await _profileService.Follow(profile, to);
            if (!followOk)
                return BadRequest(new BasicResponse { Message = "Error following" });
            await _notificationService.FollowNotification(profile, to);
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

        [HttpDelete("mobile/unfollow/{id}")]
        public async Task<ActionResult<BasicResponse>> UnFollow(Guid id, [FromBody] Profile profile)
        {
            var resultOk = await _profileService.UnFollow(profile, id);
            if (resultOk)
                return Ok(new BasicResponse { Success = true, Message = "Unfollow successful" });
            return BadRequest(new BasicResponse { Message = "Unfollow failed" });
        }

        [HttpGet("get/followers/{id}")]
        public async Task<ActionResult<List<Followings>>> GetFollowers(Guid id)
        {
            return Ok(await _profileService.GetFollowersAsync(id));
        }

        [HttpGet("mobile/get/followers/{id}")]
        public async Task<ActionResult<List<Followings>>> MobileGetFollowers(Guid id)
        {
            return Ok(await _profileService.GetFollowersAsync(id));
        }

        [HttpGet("get/followings/{id}")]
        public async Task<ActionResult<List<Profile>>> GetFollowings(Guid id)
        {
            return Ok(await _profileService.GetFollowings(id));
        }

        [HttpGet("mobile/get/followings/{id}")]
        public async Task<ActionResult<List<Profile>>> MobileGetFollowings(Guid id)
        {
            return Ok(await _profileService.GetFollowings(id));
        }

        [HttpGet("get/followers/{id}/count")]
        public async Task<ActionResult<string>> GetFollowerCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowersAsync(id) });
        }

        [HttpGet("mobile/get/followers/{id}/count")]
        public async Task<ActionResult<string>> MobileGetFollowerCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowersAsync(id) });
        }

        [HttpGet("get/followings/{id}/count")]
        public async Task<ActionResult<List<Profile>>> GetFollowingCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowingCount(id) });
        }

        [HttpGet("mobile/get/followings/{id}/count")]
        public async Task<ActionResult<List<Profile>>> MobileGetFollowingCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _profileService.GetFollowingCount(id) });
        }

        //[HttpGet("get/gifts/total")]
        //public async Task<IActionResult> GetGiftTotal()
        //{
        //    if (_profile == null)
        //    {
        //        return BadRequest(new BasicResponse { Message = "Unauthorized" });
        //    }
        //    return Ok(new DataResponse { Data = await _paymentService.GetTotalGiftsAsync(_profile.Identifier) });
        //}

        [HttpGet("mobile/get/{id}/gifts/total")]
        public async Task<IActionResult> GetGiftTotal(string id)
        {
            var profile = await MobileGetProfileById(Guid.Parse(id));
            if (profile == null)
            {
                return BadRequest(new BasicResponse { Message = "Unauthorized" });
            }
            return Ok(new DataResponse { Data = await _paymentService.GetTotalGiftsAsync(profile.Value.Identifier) });
        }
    }
}
