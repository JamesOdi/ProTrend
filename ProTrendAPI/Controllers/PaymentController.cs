using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayStack.Net;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private PayStackApi PayStack { get; set; }
        private readonly string token;
        private readonly PostsService _postsService;
        private readonly IUserService _userService;

        public PaymentController(IUserService userService, IConfiguration configuration, PostsService postService)
        {
            _postsService = postService;
            _userService = userService;
            token = configuration["Payment:PaystackSK"];
            PayStack = new(token);
        }

        [HttpPost("promote")]
        public async Task<ActionResult<object>> Promote(Promotion promotion)
        {
            var profile = _userService.GetProfile();
            if (promotion.Amount == 3000)
            {
                promotion.ExpireAt = DateTime.Now.AddDays(7);
                promotion.Audience = profile.Country;
            }
            else if (promotion.Amount == 5000)
            {
                promotion.ExpireAt = DateTime.Now.AddDays(7);
                promotion.Audience = Constants.All;
            }
            else if (promotion.Amount == 10000)
            {
                promotion.ExpireAt = DateTime.Now.AddMonths(1);
                promotion.Audience = profile.Country;
            }
            else if (promotion.Amount == 20000)
            {
                promotion.ExpireAt = DateTime.Now.AddMonths(1);
                promotion.Audience = Constants.All;
            }
            else
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidAmount });

            var post = await _postsService.GetSinglePostAsync(promotion.PostId);
            if (post == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist });

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = promotion.Amount * 100,
                Email = profile.Email,
                Reference = Generate().ToString(),
                Currency = promotion.Currency.ToUpper().Trim()
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                await _postsService.PromoteAsync(profile, promotion);
                var transaction = new Transaction
                {
                    Amount = promotion.Amount,
                    ProfileId = promotion.ProfileId,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    PromotionId = promotion.Identifier,
                    Status = false
                };
                await _postsService.InsertTransactionAsync(transaction);
                return Ok(new DataResponse { Status = Constants.OK, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Status = Constants.Error, Message = response.Message });
        }

        [HttpGet("verify/{reference}")]
        public async Task<ActionResult> Verify(string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if(response.Data.Status == "success")
            {
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                if (verifyStatus != null && verifyStatus.Status)
                {
                    return Ok(new BasicResponse { Status = Constants.OK, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Status = Constants.Error, Message = response.Message });
        }

        private static int Generate()
        {
            Random r = new((int)DateTime.Now.Ticks);
            return r.Next(100000000, 999999999);
        }
    }
}
