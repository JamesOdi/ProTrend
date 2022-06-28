﻿using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("get/{id}"), Authorize]
        public async Task<ActionResult<UserProfile>> GetProfile(string id)
        {
            var profile = await _profileService.GetUserProfileAsync(id);
            if (profile == null)
                return BadRequest(new BasicResponse { Status = ResponsesTemp.Error, Message = ResponsesTemp.UserNotFound });
            return Ok(profile);
        }

        [HttpPut("update/{id}"), Authorize]
        public async Task<ActionResult<UserProfile>> UpdateProfile(string id, [FromBody] UserProfile profile)
        {
            var result = await _profileService.UpdateProfile(id, profile);
            if (result == null)
                return BadRequest(new BasicResponse { Status = ResponsesTemp.Error, Message = ResponsesTemp.UserNotFound });
            return Ok(result);
        }
    }
}
