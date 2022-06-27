using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostsService _uploadService;
        public PostController(PostsService service)
        {
            _uploadService = service;
        }

        [HttpGet("get/posts")]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            return Ok(await _uploadService.GetAllPostsAsync());
        }

        [HttpPost("add/post")]
        public async Task<ActionResult<Post>> AddPost(Post upload)
        {
            return Ok(await _uploadService.AddPostAsync(upload));
        }

        [HttpGet("get/{id}/post")]
        public async Task<ActionResult<Post>> GetPost(string postId)
        {
            return Ok(await _uploadService.GetSinglePostAsync(postId));
        }

        [HttpGet("get/{id}/posts")]
        public async Task<ActionResult<List<Post>>> GetUserPosts(string userId)
        {
            return Ok(await _uploadService.GetUserPostsAsync(userId));
        }

        [HttpGet("get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> GetLikes(string id)
        {
            return Ok(await _uploadService.GetPostLikesAsync(id));
        }

        [HttpPost("add/like")]
        public async Task<IActionResult> AddLike(Like like)
        {
            await _uploadService.AddLikeAsync(like);
            return Ok("Liked successfully!");
        }

        [HttpGet("get/{id}/like/count")]
        public async Task<ActionResult<int>> GetLikesCount(string id)
        {
            return Ok(await _uploadService.GetLikesCountAsync(id));
        }

        [HttpPost("add/comment")]
        public async Task<ActionResult<Comment>> AddComment(Comment comment)
        {
            return Ok(await _uploadService.InsertCommentAsync(comment));
        }

        [HttpGet("get/{id}/comments")]
        public async Task<ActionResult<List<Comment>>> GetComments(string id)
        {
            return Ok(await _uploadService.GetCommentsAsync(id));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            await _uploadService.DeletePostAsync(id);
            return Ok(new BasicResponse { Status = "ok", Message = "Post deleted!" });
        }
    }
}