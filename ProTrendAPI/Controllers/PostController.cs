using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/post")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class PostController : BaseController
    {
        public PostController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/all")]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            return Ok(await _postsService.GetAllPostsAsync());
        }

        [HttpGet("get/promotions/all")]
        public async Task<ActionResult<List<Promotion>>> GetPromotions()
        {
            return Ok(await _postsService.GetPromotionsAsync(_profile));
        }

        [HttpGet("get/{id}/gift/profiles")]
        public async Task<ActionResult<List<Profile>>> GetGifters(Guid id)
        {
            return await _postsService.GetGiftersAsync(id);
        }

        [HttpPost("add/post")]
        public async Task<ActionResult<Post>> AddPost(Post upload)
        {
            if (_profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is Unauthorized" });
            upload.ProfileId = _profile.Id;
            upload.AcceptGift = false;
            upload.Disabled = false;
            return Ok(await _postsService.AddPostAsync(upload));
        }

        [HttpGet("get/{id}/post")]
        public async Task<ActionResult<Post>> GetPost(Guid id)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post == null)
                return BadRequest(new BasicResponse { Message = Constants.PostNotExist });
            return Ok(post);
        }

        [HttpGet("get/{id}/posts")]
        public async Task<ActionResult<List<Post>>> GetUserPosts(Guid id)
        {
            return Ok(await _postsService.GetUserPostsAsync(id));
        }

        [HttpGet("get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> GetLikes(Guid id)
        {
            return Ok(await _postsService.GetPostLikesAsync(id));
        }

        [HttpPost("add/like")]
        public async Task<IActionResult> AddLike(Like like)
        {
            var post = await _postsService.GetSinglePostAsync(like.UploadId);
            if (post != null)
            {
                like.SenderId = _profile.Id;
                await _postsService.AddLikeAsync(like);
                await _notificationService.LikeNotification(_profile, post.ProfileId);
                return Ok(new BasicResponse { Success = true, Message = Constants.Success });
            }
            return BadRequest(new BasicResponse { Message = Constants.PostNotExist });
        }

        [HttpGet("get/{id}/like/count")]
        public async Task<ActionResult<int>> GetLikesCount(Guid id)
        {
            return Ok(await _postsService.GetLikesCountAsync(id));
        }

        [HttpPost("add/comment")]
        public async Task<ActionResult<Comment>> AddComment(Comment comment)
        {
            var post = await _postsService.GetSinglePostAsync(comment.PostId);
            if (post != null)
            {
                comment.UserId = _profile.Id;
                comment.Identifier = comment.Id;
                await _notificationService.CommentNotification(_profile, post.ProfileId);
                var commentResult = await _postsService.InsertCommentAsync(comment);
                return Ok(commentResult);
            }
            return BadRequest(new BasicResponse { Message = Constants.PostNotExist });
        }

        [HttpGet("get/{id}/gifts")]
        public async Task<ActionResult> GetAllGiftsOnPost(Guid id)
        {
            return Ok(await _postsService.GetAllGiftOnPostAsync(id));
        }

        [HttpGet("get/{id}/comments")]
        public async Task<ActionResult<List<Comment>>> GetComments(Guid id)
        {
            return Ok(await _postsService.GetCommentsAsync(id));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var delete = await _postsService.DeletePostAsync(id);
            if (!delete)
                return BadRequest(new BasicResponse { Message = Constants.PDError });
            return Ok(new BasicResponse { Success = true, Message = "Post deleted" });
        }
    }
}