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

        [HttpGet("get")]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            return Ok(await _postsService.GetAllPostsAsync());
        }

        [HttpGet("get/{page}")]
        public async Task<ActionResult<List<Post>>> GetPostsPaginated(int page)
        {
            return Ok(await _postsService.GetPagePostsAsync(page));
        }

        [HttpGet("mobile/get/{page}")]
        public async Task<ActionResult<List<Post>>> MobileGetPostsPaginated(int page)
        {
            return Ok(await _postsService.GetPagePostsAsync(page));
        }

        [HttpGet("get/promotions")]
        public async Task<ActionResult<List<Promotion>>> GetPromotions()
        {
            return Ok(await _postsService.GetPromotionsAsync(_profile));
        }

        [HttpGet("mobile/get/promotions")]
        public async Task<ActionResult<List<Promotion>>> MobileGetPromotions()
        {
            return Ok(await _postsService.GetPromotionsAsync(_profile));
        }

        [HttpGet("get/{id}/gift/profiles")]
        public async Task<ActionResult<List<Profile>>> GetGifters(Guid id)
        {
            return await _postsService.GetGiftersAsync(id);
        }

        [HttpGet("mobile/get/{id}/gift/profiles")]
        public async Task<ActionResult<List<Profile>>> MobileGetGifters(Guid id)
        {
            return await _postsService.GetGiftersAsync(id);
        }

        [HttpPost("add")]
        public async Task<ActionResult<DataResponse>> AddPost(Post upload)
        {
            upload.ProfileId = _profile.Id;
            upload.AcceptGift = false;
            upload.Disabled = false;
            var uploadResult = await _postsService.AddPostAsync(upload);
            return Ok(new { Success = true, Data = uploadResult });
        }

        [HttpPost("mobile/add")]
        public async Task<ActionResult<object>> AddPostFromMobile([FromBody] Post upload)
        {
            upload.AcceptGift = false;
            upload.Disabled = false;
            var uploadResult = await _postsService.AddPostAsync(upload);
            return Ok(new { Success = true, Data = uploadResult });
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<Post>> GetPost(Guid id)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post == null)
                return BadRequest(new BasicResponse { Message = Constants.PostNotExist });
            return Ok(post);
        }

        [HttpGet("mobile/get/{id}")]
        public async Task<ActionResult<Post>> MobileGetPost(Guid id)
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

        [HttpGet("mobile/get/{id}/posts")]
        public async Task<ActionResult<List<Post>>> MobileGetUserPosts(Guid id)
        {
            return Ok(await _postsService.GetUserPostsAsync(id));
        }

        [HttpGet("get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> GetLikes(Guid id)
        {
            return Ok(await _postsService.GetPostLikesAsync(id));
        }

        [HttpGet("mobile/get/{id}/likes")]
        public async Task<ActionResult<List<Like>>> MobileGetLikes(Guid id)
        {
            return Ok(await _postsService.GetPostLikesAsync(id));
        }

        [HttpPost("add/like/{id}")]
        public async Task<IActionResult> AddLike(Guid id)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post != null)
            {
                var like = new Like { SenderId = _profile.Identifier, Time = DateTime.Now, UploadId = id };
                var liked = await _postsService.AddLikeAsync(like);
                var notiSent = await _notificationService.LikeNotification(_profile, post.ProfileId);
                if (liked && notiSent)
                    return Ok(new BasicResponse { Success = true, Message = "Post liked" });
            }
            return BadRequest(new BasicResponse { Message = "Error liking post" });
        }

        [HttpPost("mobile/add/like/{id}")]
        public async Task<IActionResult> AddLike(Guid id, [FromBody] Profile profile)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post != null)
            {
                var like = new Like { SenderId = profile.Identifier, Time = DateTime.Now, UploadId = id };
                var liked = await _postsService.AddLikeAsync(like);
                var notiSent = await _notificationService.LikeNotification(profile, post.ProfileId);
                if (liked && notiSent)
                    return Ok(new BasicResponse { Success = true, Message = "Post liked" });
            }
            return BadRequest(new BasicResponse { Message = "Error liking post" });
        }

        [HttpDelete("delete/like/{id}")]
        public async Task<IActionResult> RemoveLike(Guid id)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post != null)
            {
                var liked = await _postsService.RemoveLike(id, _profile.Identifier);
                if (liked)
                    return Ok(new BasicResponse { Success = true, Message = "Post unliked" });
            }
            return BadRequest(new BasicResponse { Message = "Error unliking post" });
        }

        [HttpDelete("mobile/delete/like/{id}")]
        public async Task<IActionResult> RemoveLikeFromMobile(Guid id, [FromBody] Profile profile)
        {
            var post = await _postsService.GetSinglePostAsync(id);
            if (post != null)
            {
                var liked = await _postsService.RemoveLike(id, profile.Identifier);
                if (liked)
                    return Ok(new BasicResponse { Success = true, Message = "Post unliked" });
            }
            return BadRequest(new BasicResponse { Message = "Error unliking post" });
        }

        [HttpGet("get/{id}/like/count")]
        public async Task<ActionResult<int>> GetLikesCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _postsService.GetLikesCountAsync(id) });
        }

        [HttpGet("mobile/get/{id}/like/count")]
        public async Task<ActionResult<int>> MobileGetLikesCount(Guid id)
        {
            return Ok(new DataResponse { Status = Constants.OK, Data = await _postsService.GetLikesCountAsync(id) });
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

        [HttpPost("mobile/{id}/add/comment")]
        public async Task<ActionResult<Comment>> AddCommentFromMobile(string id, Comment comment)
        {
            var profile = await _profileService.GetProfileByIdAsync(Guid.Parse(id));
            var post = await _postsService.GetSinglePostAsync(comment.PostId);
            if (post != null)
            {
                comment.UserId = profile.Id;
                comment.Identifier = comment.Id;
                await _notificationService.CommentNotification(profile, post.ProfileId);
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

        [HttpGet("mobile/get/{id}/gifts")]
        public async Task<ActionResult> MobileGetAllGiftsOnPost(Guid id)
        {
            return Ok(await _postsService.GetAllGiftOnPostAsync(id));
        }

        [HttpGet("get/{id}/comments")]
        public async Task<ActionResult<List<Comment>>> GetComments(Guid id)
        {
            return Ok(await _postsService.GetCommentsAsync(id));
        }

        [HttpGet("mobile/get/{id}/comments")]
        public async Task<ActionResult<List<Comment>>> MobileGetComments(Guid id)
        {
            return Ok(await _postsService.GetCommentsAsync(id));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var delete = await _postsService.DeletePostAsync(id, _profile.Identifier);
            if (!delete)
                return BadRequest(new BasicResponse { Message = Constants.PDError });
            return Ok(new BasicResponse { Success = true, Message = "Post deleted" });
        }

        [HttpDelete("mobile/delete/{id}")]
        public async Task<IActionResult> DeleteMobilePost(Guid id, [FromBody] Profile profile)
        {
            var delete = await _postsService.DeletePostAsync(id, profile.Identifier);
            if (!delete)
                return BadRequest(new BasicResponse { Message = Constants.PDError });
            return Ok(new BasicResponse { Success = true, Message = "Post deleted" });
        }
    }
}
