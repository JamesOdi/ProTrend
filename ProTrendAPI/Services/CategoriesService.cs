using ProTrendAPI.Models;
using Microsoft.Extensions.Options;
using ProTrendAPI.Settings;
using MongoDB.Driver;

namespace ProTrendAPI.Services
{
    public class CategoriesService : BaseService
    {
        public CategoriesService(IOptions<DBSettings> settings): base(settings)
        {}

        public async Task<Category> AddCategoryAsync(string name)
        {
            var category = await GetSingleCategory(name);
            if (category != null)
            {
                return category;
            }
            category = new Category { Name = name };
            
            await _categoriesCollection.InsertOneAsync(category);
            return category;
        }

        public async Task<Category?> GetSingleCategory(string name)
        {
            return await _categoriesCollection.Find(Builders<Category>.Filter.Where(category => category.Name == name)).FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync(string name)
        {
            return await _categoriesCollection.Find(Builders<Category>.Filter.Where(category => category.Name.Contains(name))).ToListAsync();
        }
    }
}
