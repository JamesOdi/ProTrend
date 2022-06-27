using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;
        public SearchController(SearchService service)
        {
            _searchService = service;
        }

        [HttpGet("{search}")]
        public async Task<ActionResult<List<List<string>>>> GetSearchResults(string search)
        {
            return Ok(await _searchService.GetSearchResultAsync(search));
        }

        [HttpGet("posts/{name}")]
        public async Task<ActionResult<List<string>>> GetPosts(string name)
        {
            return Ok(await _searchService.GetPostsWithNameAsync(name));
        }

        [HttpGet("people/{name}")]
        public async Task<ActionResult<List<string>>> GetPeople(string name)
        {
            return Ok(await _searchService.GetProfilesWithNameAsync(name));
        }

        [HttpGet("category/{name}")]
        public async Task<ActionResult<List<string>>> GetPostsInCategory(string name)
        {
            return Ok(await _searchService.GetPostsInCategoryAsync(name));
        }

        //[HttpGet("")]
        //public async Task<ActionResult<List<string>>> GetRelatedSearchAsync(string name)
        //{

        //}
    }
}
