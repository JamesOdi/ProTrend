using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/category")]
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

        [HttpPost("add/{name}")]
        public async Task<ActionResult<Category>> AddCategory(string name)
        {
            return Ok(await _categoriesService.AddCategoryAsync(name));
        }

        [HttpGet("get/{name}")]
        public async Task<ActionResult<List<Category>>> GetCategories(string name)
        {
            return Ok(await _categoriesService.GetCategoriesAsync(name));
        }

        [HttpGet("mobile/get/{name}/1")]
        public async Task<ActionResult<Category>> GetMobileCategory(string name)
        {
            var category = await _categoriesService.GetSingleCategory(name);
            if (category == null)
                return BadRequest(new BasicResponse { Message = Constants.CatNotExist });
            return Ok(category);
        }

        [HttpPost("mobile/add/{name}")]
        public async Task<ActionResult<Category>> AddMobileCategory(string name)
        {
            return Ok(await _categoriesService.AddCategoryAsync(name));
        }

        [HttpGet("mobile/get/{name}")]
        public async Task<ActionResult<List<Category>>> GetMobileCategories(string name)
        {
            return Ok(await _categoriesService.GetCategoriesAsync(name));
        }
    }
}