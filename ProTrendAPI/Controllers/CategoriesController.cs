using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models.Response;
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
        public async Task<ActionResult<ActionResponse>> GetCategory(string name)
        {
            var category = await _categoriesService.GetSingleCategory(name);
            if (category == null)
                return NotFound(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = category });
        }

        [HttpPost("add/{name}")]
        public async Task<ActionResult<ActionResponse>> AddCategory(string name)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = await _categoriesService.AddCategoryAsync(name) });
        }

        [HttpGet("get/{name}")]
        public async Task<ActionResult<ActionResponse>> GetCategories(string name)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = await _categoriesService.GetCategoriesAsync(name) });
        }

        [HttpGet("mobile/get/{name}/1")]
        public async Task<ActionResult<ActionResponse>> GetMobileCategory(string name)
        {
            var category = await _categoriesService.GetSingleCategory(name);
            if (category == null)
                return NotFound(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = category });
        }

        [HttpPost("mobile/add/{name}")]
        public async Task<ActionResult<ActionResponse>> AddMobileCategory(string name)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = await _categoriesService.AddCategoryAsync(name) });
        }

        [HttpGet("mobile/get/{name}")]
        public async Task<ActionResult<ActionResponse>> GetMobileCategories(string name)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = await _categoriesService.GetCategoriesAsync(name) });
        }
    }
}