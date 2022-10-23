using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/search")]
    [ApiController]
    [ProTrndAuthorizationFilter]
    public class SearchController : BaseController
    {
        public SearchController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/{search}")]
        public async Task<ActionResult<object>> GetSearchResults(string search)
        {
            return Ok(await _searchService.GetSearchResultAsync(search));
        }

        [HttpGet("get/posts/{name}")]
        public async Task<ActionResult<List<string>>> GetPosts(string name)
        {
            return Ok(await _searchService.SearchPostsByNameAsync(name));
        }

        [HttpGet("get/people/{name}")]
        public async Task<ActionResult<List<string>>> GetPeople(string name)
        {
            return Ok(await _searchService.SearchProfilesByNameAsync(name));
        }

        [HttpGet("get/email/{email}")]
        public async Task<ActionResult<List<Profile>>> GetProfileByEmail(string email)
        {
            return Ok(await _searchService.SearchProfilesByEmailAsync(email.ToLower()));
        }

        [HttpGet("category/{name}")]
        public async Task<ActionResult<List<string>>> GetPostsInCategory(string name)
        {
            return Ok(await _searchService.SearchPostsByCategoryAsync(name));
        }
    }
}
