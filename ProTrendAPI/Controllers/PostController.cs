using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly PostsService _uploadService;
        private readonly IUserService _userService;
        private readonly string currentUserId;
        public PostController(PostsService service, IUserService userService)
        {
            _uploadService = service;
            _userService = userService;
            currentUserId = _userService.GetUserProfile().Id;
        }

        [HttpGet("get/all")]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            return Ok(await _uploadService.GetAllPostsAsync());
        }

        [HttpPost("add/post")]
        public async Task<ActionResult<Post>> AddPost(Post upload)
        {
            upload.UserId = currentUserId;
            return Ok(await _uploadService.AddPostAsync(upload));
        }

        [HttpGet("get/{id}/post")]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            var post = await _uploadService.GetSinglePostAsync(id);
            if (post == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist });
            return Ok(post);
        }

        [HttpGet("get/{id}/posts")]
        public async Task<ActionResult<List<Post>>> GetUserPosts(string id)
        {
            return Ok(await _uploadService.GetUserPostsAsync(id));
        }

        [HttpGet("get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> GetLikes(string id)
        {
            return Ok(await _uploadService.GetPostLikesAsync(id));
        }

        [HttpPost("add/like")]
        public async Task<IActionResult> AddLike(Like like)
        {
            like.UserId = currentUserId;
            await _uploadService.AddLikeAsync(like);
            return Ok(new BasicResponse { Status = Constants.OK, Message = Constants.Success });
        }

        [HttpGet("get/{id}/like/count")]
        public async Task<ActionResult<int>> GetLikesCount(string id)
        {
            return Ok(await _uploadService.GetLikesCountAsync(id));
        }

        [HttpPost("add/comment")]
        public async Task<ActionResult<Comment>> AddComment(Comment comment)
        {
            comment.UserId = currentUserId;
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
            var delete = await _uploadService.DeletePostAsync(id);
            if (!delete)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PDError });
            return Ok(new BasicResponse { Status = Constants.OK, Message = Constants.Success });
        }
    }
}