using Microsoft.AspNetCore.Mvc;
using PayStack.Net;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class PaymentController : BaseController
    {
        private PayStackApi PayStack { get; set; }
        private readonly string token;

        public PaymentController(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            token = configuration["Payment:PaystackSK"];
            PayStack = new(token);
        }

        [HttpPost("promote")]
        public async Task<ActionResult<object>> Promote(Promotion promotion)
        {
            if (promotion.Amount == 5000)
            {
                promotion.ExpireAt = DateTime.Now.AddDays(7);
                promotion.Audience = _profile.Country;
            }
            else if (promotion.Amount == 8000)
            {
                promotion.ExpireAt = DateTime.Now.AddDays(7);
                promotion.Audience = Constants.All;
            }
            else if (promotion.Amount == 15000)
            {
                promotion.ExpireAt = DateTime.Now.AddMonths(1);
                promotion.Audience = _profile.Country;
            }
            else if (promotion.Amount == 30000)
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
                Email = _profile.Email,
                Reference = Generate().ToString(),
                Currency = promotion.Currency.ToUpper().Trim()
                //CallbackUrl = ""
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                var transaction = new Transaction
                {
                    Amount = promotion.Amount,
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = promotion.Identifier,
                    Status = false
                };
                await _postsService.InsertTransactionAsync(transaction);
                return Ok(new { Success = true, Ref = request.Reference, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("buy_gifts/{count}")]
        public async Task<ActionResult<object>> BuyGifts(int count)
        {
            if (count < 1)
                return BadRequest(new BasicResponse { Message = "Cannot but less than 1 gift" });
            var value = 500 * count;

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = value * 100,
                Email = _profile.Email,
                Reference = Generate().ToString(),
                Currency = "NGN"
                //CallbackUrl = ""
            };

            TransactionInitializeResponse response = PayStack.Transactions.Initialize(request);
            if (response.Status)
            {
                var transaction = new Transaction
                {
                    Amount = value,
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = Guid.NewGuid(),
                    Status = false
                };

                await _postsService.InsertTransactionAsync(transaction);
                return Ok(new { Success = true, Ref = request.Reference, Data = response.Data.AuthorizationUrl });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("accept_gifts/{id}")]
        public async Task<ActionResult<object>> AcceptGift(string id)
        {
            if (_profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is not Authorized" });
            var post = await _postsService.GetSinglePostAsync(Guid.Parse(id));

            TransactionInitializeRequest request = new()
            {
                AmountInKobo = 1500 * 100,
                Email = _profile.Email,
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
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = post.Identifier,
                    Status = false
                };

                var resultOk = await _postsService.InsertTransactionAsync(transaction);
                if (resultOk)
                    return Ok(new
                    {
                        Success = true,
                        Ref = request.Reference,
                        Data = response.Data.AuthorizationUrl
                    });
            }
            // CallbackUrl = response after payment url to go to in request
            return BadRequest(new BasicResponse { Message = response.Message });
        }

        [HttpPost("send_gifts/{id}/{count}")]
        public async Task<ActionResult<object>> SendGift(Guid id, int count)
        {
            if (count < 1 || count > 100)
            {
                return BadRequest(new BasicResponse { Message = Constants.InvalidAmount });
            }
            var totalGifts = await _postsService.GetTotalGiftsAsync(_profile.Identifier);
            if (totalGifts < count)
                return BadRequest(new BasicResponse { Message = "Insufficient Gifts" });
            var post = await _postsService.GetSinglePostAsync(id);
            if (post == null || !post.AcceptGift || post.ProfileId == _profile.Identifier)
                return BadRequest(new BasicResponse { Message = "Error accessing post" });

            var sent = await _postsService.SendGiftToPostAsync(post, count, _profile.Identifier);
            if (sent < 1)
                return BadRequest(new BasicResponse { Success = false, Message = "Error sending gift" });

            var transaction = new Transaction
            {
                Amount = count,
                ProfileId = _profile.Identifier,
                CreatedAt = DateTime.Now,
                TrxRef = Generate().ToString(),
                ItemId = id,
                Status = true
            };

            var responseOk = await _postsService.InsertTransactionAsync(transaction);
            var notificationSent = await _notificationService.SendGiftNotification(_profile, post, count);
            if (responseOk && notificationSent)
                return Ok(new { Success = true, Message = sent + " gift sent" });
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

        [HttpGet("verify/promotion/{reference}")]
        public async Task<ActionResult> Verify(string reference, [FromBody] Promotion promotion)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                if (verifyStatus != null && verifyStatus.Status)
                {
                    var promotionOk = await _postsService.PromoteAsync(_profile, promotion);
                    if (promotionOk)
                        return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = "Error verifying paid promotion" });
        }

        [HttpGet("verify/accept_gift/{id}/{reference}")]
        public async Task<ActionResult> VerifyAcceptGift(Guid id, string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                if (verifyStatus != null && verifyStatus.Status)
                {
                    var resultOk = await _postsService.AcceptGift(id);
                    if (resultOk)
                        return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = "Error verifying payment" });
        }

        [HttpGet("verify/purchase/gift/{reference}")]
        public async Task<ActionResult> VerifyGiftPurchase(string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var count = response.Data.Amount / 50000;
                var transaction = await _postsService.GetTransactionByRefAsync(reference);
                var verifyStatus = await _postsService.VerifyTransactionAsync(transaction);
                if (verifyStatus != null && verifyStatus.Status)
                {
                    var giftsBought = await _postsService.BuyGiftsAsync(_profile.Identifier, count);
                    if (giftsBought)
                        return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = "Error verifying payment" });
        }

        private static int Generate()
        {
            Random r = new((int)DateTime.Now.Ticks);
            return r.Next(100000000, 999999999);
        }
    }
}
