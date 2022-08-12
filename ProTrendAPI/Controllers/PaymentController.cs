using Microsoft.AspNetCore.Mvc;
using PayStack.Net;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class PaymentController : BaseController
    {
        private PayStackApi PayStack { get; set; }
        private readonly string token;

        public PaymentController(IServiceProvider serviceProvider, IConfiguration configuration):base(serviceProvider)
        {
            token = configuration["Payment:PaystackSK"];
            PayStack = new(token);
        }

        [HttpPost("promote")]
        public async Task<ActionResult<object>> Promote(Promotion promotion)
        {
            var profile = _userService.GetProfile();
            if (profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is not Authorized" });
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
                return BadRequest(new BasicResponse { Message = Constants.InvalidAmount });

            var post = await _postsService.GetSinglePostAsync(promotion.PostId);
            if (post == null)
                return BadRequest(new BasicResponse { Message = Constants.PostNotExist });

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = promotion.Amount * 100,
                Email = profile.Email,
                Reference = Generate().ToString(),
                Currency = promotion.Currency.ToUpper().Trim()
                //CallbackUrl = ""
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                await _postsService.PromoteAsync(profile, promotion);
                var transaction = new Transaction
                {
                    Amount = promotion.Amount,
                    ProfileId = profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = promotion.Identifier,
                    Status = false
                };
                await _postsService.InsertTransactionAsync(transaction);
                return Ok(new DataResponse { Status = Constants.OK, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("buy_gifts/{count}")]
        public async Task<ActionResult<object>> BuyGifts(int count)
        {
            var profile = _userService.GetProfile();
            if (profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is not Authorized" });
            if (count < 1)
                return BadRequest(new BasicResponse { Message = "Cannot but less than 1 gift" });
            var value = 500 * count;

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = value * 100,
                Email = profile.Email,
                Reference = Generate().ToString(),
                Currency = "NGN"
                //CallbackUrl = ""
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                var gift = new Gift { ProfileId = profile.Identifier };
                gift.Identifier = gift.Id;
                var transaction = new Transaction
                {
                    Amount = value,
                    ProfileId = profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = gift.Identifier,
                    Status = false
                };

                await _postsService.InsertTransactionAsync(transaction);
                return Ok(new DataResponse { Status = Constants.OK, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("accept_gifts/{id}")]
        public async Task<ActionResult<object>> AcceptGift(string id)
        {
            var profile = _userService.GetProfile();
            if (profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is not Authorized" });
            var post = await _postsService.GetSinglePostAsync(Guid.Parse(id));
            if (post == null || !post.AcceptGift)
                return BadRequest(new BasicResponse { Message = "Post does not accept gifts" });

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = 1500 * 100,
                Email = profile.Email,
                Reference = Generate().ToString(),
                Currency = "NGN"
                //CallbackUrl = ""
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                var transaction = new Transaction
                {
                    Amount = 1500,
                    ProfileId = profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = post.Identifier,
                    Status = false
                };

                await _postsService.InsertTransactionAsync(transaction);
                var resultOk = await _postsService.AcceptGift(Guid.Parse(id));
                if (resultOk)
                    return Ok(new DataResponse { Status = Constants.OK, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("send_gifts/{id}/{count}")]
        public async Task<ActionResult<object>> SendGift(Guid id, int count)
        {
            var profile = _userService.GetProfile();
            if (profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is not Authorized" });
            if (count < 1)
            {
                return BadRequest(new BasicResponse { Message = Constants.InvalidAmount });
            }
            var totalGifts = await _postsService.GetTotalGiftsAsync(profile.Identifier);
            if (totalGifts < 1)
                return BadRequest(new BasicResponse { Message = "Insufficient Gifts" });
            var post = await _postsService.GetSinglePostAsync(id);
            if (post == null || !post.AcceptGift)
                return BadRequest(new BasicResponse { Message = Constants.PostNotExist });

            await _postsService.SendGiftToPostAsync(post, count, profile.Identifier);

            var transaction = new Transaction
            {
                Amount = count,
                ProfileId = profile.Identifier,
                CreatedAt = DateTime.Now,
                TrxRef = Generate().ToString(),
                ItemId = id,
                Status = true
            };

            var responseOk = await _postsService.InsertTransactionAsync(transaction);
            await _notificationService.SendGiftNotification(profile, post);
            if (responseOk)
                return Ok(new { Success = true, Message = "Gift sent" });
            return BadRequest(new { Success = false, Message = "Error sending gift" });
        }

        [HttpPost("withdraw/{total}")]
        public async Task<IActionResult> RequestWithdrawal(int total)
        {
            var success = await _postsService.RequestWithdrawalAsync(_userService.GetProfile(), total);
            if (success)
                return BadRequest(new { Success = false, Message = "Error requesting withdrawal" });
            return Ok(new { Success = true, Message = "Request sent" });
        }

        [HttpGet("verify/{reference}")]
        public async Task<ActionResult> Verify(string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                if (verifyStatus != null && verifyStatus.Status)
                {
                    return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpGet("verify/purchase/gift/{count}/{reference}")]
        public async Task<ActionResult> VerifyGiftPurchase(int count, string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                var profile = _userService.GetProfile();
                if (verifyStatus != null && verifyStatus.Status && profile != null)
                {
                    await _postsService.BuyGiftsAsync(profile.Identifier, count);
                    return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        private static int Generate()
        {
            Random r = new((int)DateTime.Now.Ticks);
            return r.Next(100000000, 999999999);
        }
    }
}
