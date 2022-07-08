using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models.User;
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
        private readonly Profile profile;
        private readonly NotificationService _notificationService;
        public PostController(PostsService service, NotificationService notificationService, IUserService userService)
        {
            _notificationService = notificationService;
            _uploadService = service;
            _userService = userService;
            profile = _userService.GetProfile();
        }

        [HttpGet("get/all")]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            return Ok(await _uploadService.GetAllPostsAsync());
        }

        //[HttpGet("get/promotions/all")]
        //public async Task<ActionResult<List<Promotion>>> GetPromotions()
        //{

        //}

        [HttpPost("add/post")]
        public async Task<ActionResult<Post>> AddPost(Post upload)
        {
            upload.UserId = profile.Id;
            return Ok(await _uploadService.AddPostAsync(upload));
        }

        [HttpGet("get/{id}/post")]
        public async Task<ActionResult<Post>> GetPost(Guid id)
        {
            var post = await _uploadService.GetSinglePostAsync(id);
            if (post == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist });
            return Ok(post);
        }

        [HttpGet("get/{id}/posts")]
        public async Task<ActionResult<List<Post>>> GetUserPosts(Guid id)
        {
            return Ok(await _uploadService.GetUserPostsAsync(id));
        }

        [HttpGet("get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> GetLikes(Guid id)
        {
            return Ok(await _uploadService.GetPostLikesAsync(id));
        }

        [HttpPost("add/like")]
        public async Task<IActionResult> AddLike(Like like)
        {
            var post = await _uploadService.GetSinglePostAsync(like.UploadId);
            if (post != null)
            {
                like.SenderId = profile.Id;
                await _uploadService.AddLikeAsync(like);
                await _notificationService.LikeNotification(profile, post.UserId);
                return Ok(new BasicResponse { Message = Constants.Success });
            }
            return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist });
        }

        [HttpGet("get/{id}/like/count")]
        public async Task<ActionResult<int>> GetLikesCount(Guid id)
        {
            return Ok(await _uploadService.GetLikesCountAsync(id));
        }

        [HttpPost("add/comment")]
        public async Task<ActionResult<Comment>> AddComment(Comment comment)
        {
            var post = await _uploadService.GetSinglePostAsync(comment.PostId);
            if (post != null)
            {
                comment.UserId = profile.Id;
                comment.Identifier = comment.Id;
                await _notificationService.CommentNotification(profile, post.UserId);
                var commentResult = await _uploadService.InsertCommentAsync(comment);
                return Ok(commentResult);
            }
            return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist });
        }

        [HttpGet("get/{id}/comments")]
        public async Task<ActionResult<List<Comment>>> GetComments(Guid id)
        {
            return Ok(await _uploadService.GetCommentsAsync(id));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var delete = await _uploadService.DeletePostAsync(id);
            if (!delete)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PDError });
            return Ok(new BasicResponse { Message = Constants.Success });
        }
    }
}