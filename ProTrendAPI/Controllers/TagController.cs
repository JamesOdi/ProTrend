using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;
using MongoDB.Driver;
using ProTrendAPI.Models;
using Tag = ProTrendAPI.Models.Tag;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return BadRequest(new BasicResponse { Status = ResponsesTemp.Error, Message = ResponsesTemp.InvalidTag });
            return Ok(tags);
        }

        [HttpPost("add/{name}")]
        public async Task<IActionResult> AddTag(string name)
        {
            await _tagsService.AddTagAsync(name);
            return Ok(new BasicResponse { Status = ResponsesTemp.OK, Message = ResponsesTemp.ResultOk });
        }
    }
}
