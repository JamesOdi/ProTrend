using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;
using Tag = ProTrendAPI.Models.Posts.Tag;
using Microsoft.AspNetCore.Authorization;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly TagsService _tagsService;
        public TagController(TagsService tagService)
        {
            _tagsService = tagService;
        }

        [HttpGet("get/{name}")]
        public async Task<ActionResult<List<Tag>>> GetTags(string name)
        {
            var tags = await _tagsService.GetTagsWithNameAsync(name);
            if (tags == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidTag });
            return Ok(tags);
        }

        [HttpPost("add/{name}")]
        public async Task<IActionResult> AddTag(string name)
        {
            await _tagsService.AddTagAsync(name);
            return Ok(new BasicResponse { Message = Constants.Success });
        }
    }
}
