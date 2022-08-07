using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models.User;
using ProTrendAPI.Services.Network;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;
        public SearchController(SearchService service)
        {
            _searchService = service;
        }

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
