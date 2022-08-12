using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class CategoriesController : BaseController
    {
        
        public CategoriesController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/{name}/1")]
        public async Task<ActionResult<Category>> GetCategory(string name)
        {
            var category = await _categoriesService.GetSingleCategory(name);
            if (category == null)
                return BadRequest(new BasicResponse { Message = Constants.CatNotExist });
            return Ok(category);
        }

        [HttpPost("add")]
        public async Task<ActionResult<Category>> AddCategory(string name)
        {
            return Ok(await _categoriesService.AddCategoryAsync(name));
        }

        [HttpGet("get/{name}")]
        public async Task<ActionResult<List<Category>>> GetCategories(string name)
        {
            return Ok(await _categoriesService.GetCategoriesAsync(name));
        }
    }
}