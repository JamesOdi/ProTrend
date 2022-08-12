using Microsoft.AspNetCore.Mvc;
using Tag = ProTrendAPI.Models.Posts.Tag;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class TagController : BaseController
    {
        public TagController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/{name}")]
        public async Task<ActionResult<List<Tag>>> GetTags(string name)
        {
            var tags = await _tagsService.GetTagsWithNameAsync(name);
            if (tags == null)
                return BadRequest(new BasicResponse { Message = Constants.InvalidTag });
            return Ok(tags);
        }

        [HttpPost("add/{name}")]
        public async Task<IActionResult> AddTag(string name)
        {
            await _tagsService.AddTagAsync(name);
            return Ok(new BasicResponse { Message = "Tag added" });
        }
    }
}
