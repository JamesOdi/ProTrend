using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoriesService _categoriesService;
        public CategoriesController(CategoriesService service)
        {
            _categoriesService = service;
        }

        [HttpGet("get/{name}/1")]
        public async Task<ActionResult<Category>> GetCategory(string name)
        {
            return Ok(await _categoriesService.GetSingleCategory(name));
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