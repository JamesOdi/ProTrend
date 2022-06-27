using Microsoft.Extensions.Options;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;
using MongoDB.Driver;
using Tag = ProTrendAPI.Models.Tag;

namespace ProTrendAPI.Services
{
    public class TagsService: BaseService
    {
        public TagsService(IOptions<DBSettings> options): base(options) {}

        public async Task<List<Tag>?> GetTagsWithNameAsync(string name)
        {
            if (!name.StartsWith("#") || name.Contains(' '))
            {
                return null;
            }
            return await _tagsCollection.Find(Builders<Tag>.Filter.Where(t => t.Name.Contains(name.ToLower()))).ToListAsync();
        }

        public async Task AddTagAsync(string name)
        {
            if (!name.StartsWith("#") || name.Contains(' '))
            {
                return;
            }
            var tag = await TagExists(name);
            if (tag != null)
                return;
            await _tagsCollection.InsertOneAsync(new Tag { Name = name.ToLower() });
            return;
        }

        private async Task<Tag?> TagExists(string name)
        {
            return await _tagsCollection.Find(t => t.Name.Equals(name.ToLower())).FirstOrDefaultAsync();
        }
    }
}
