using Microsoft.AspNetCore.Mvc;
using PayStack.Net;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [ProTrndAuthorizationFilter]
    public class PaymentController : BaseController
    {
        private PayStackApi PayStack { get; set; }
        private readonly string token;
        private readonly string accencrypttoken;

        public PaymentController(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            token = configuration["Payment:PaystackSK"];
            accencrypttoken = configuration["Payment:AccEncrypt"];
            PayStack = new(token);
        }

        [HttpPost("add/account")]
        public async Task<ActionResult<ActionResponse>> AddAccount(AccountDetailsDTO account)
        {
            account.ProfileId = _profile.Identifier;
            return Ok(new ActionResponse { StatusCode = 200, Successful = true, Message = "Account Added", Data = await _paymentService.AddAccountDetailsAsync(account, accencrypttoken) });
        }

        [HttpPost("buy_gifts/balance/{count}")]
        public async Task<ActionResult<object>> BuyGifts(int count)
        {
            var trxRef = Generate().ToString();
            if (count < 1)
                return BadRequest(new BasicResponse { Message = "Cannot buy less than 1 gift" });
            var value = 500 * count;

            var totalBalance = await _paymentService.GetTotalBalance(_profile.Identifier);
            if (totalBalance < 0 && totalBalance <= value)
                return BadRequest(new ActionResponse { Message = "Error buying gifts" });

                var transaction = new Transaction
                {
                    Amount = value,
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = trxRef,
                    ItemId = Guid.NewGuid()
                };

                await _paymentService.InsertTransactionAsync(transaction);
                return Ok(new { Success = true, Ref = trxRef, Data = "Sucess" });
        }

        [HttpPost("balance")]
        public async Task<ActionResult<int>> GetTotalBalance()
        {
            return Ok(await _paymentService.GetTotalBalance(_profile.Identifier));
        }

        [HttpPost("mobile/balance")]
        public async Task<ActionResult<int>> GetTotalBalance(Profile profile)
        {
            return Ok(await _paymentService.GetTotalBalance(profile.Identifier));
        }

        [HttpPost("top_up/balance/{total}")]
        public async Task<ActionResult<object>> TopUpBalance(int  total)
        {
            TransactionInitializeRequest request = new()
            {
                AmountInKobo = total * 100,
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
                    Amount = total,
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = request.Reference,
                    ItemId = Guid.NewGuid()
                };

                await _paymentService.InsertTransactionAsync(transaction);
                return Ok(new { Success = true, Ref = request.Reference, Data = response.Data.AuthorizationUrl });
            }
            return BadRequest(new {Success = false, Ref = request.Reference, Data = "Error making transaction" });
        }

        [HttpPost("send_gifts/{id}/{count}")]
        public async Task<ActionResult<object>> SendGift(Guid id, int count)
        {
            if (count < 1 || count > 100)
            {
                return BadRequest(new BasicResponse { Message = Constants.InvalidAmount });
            }
            var totalGifts = await _paymentService.GetTotalGiftsAsync(_profile.Identifier);
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
                ItemId = id
            };

            var responseOk = await _paymentService.InsertTransactionAsync(transaction);
            var notificationSent = await _notificationService.SendGiftNotification(_profile, post, count);
            if (responseOk && notificationSent)
                return Ok(new { Success = true, Message = sent + " gift sent" });
            return BadRequest(new { Success = false, Message = "Error sending gift" });
        }

        [HttpPost("withdraw/balance/{total}")]
        public async Task<IActionResult> RequestWithdrawal(int total)
        {
            var success = await _paymentService.RequestWithdrawalAsync(_profile, total);
            if (success)
                return BadRequest(new { Success = false, Message = "Error requesting withdrawal" });
            return Ok(new { Success = true, Message = "Request sent" });
        }

        //[HttpPost("verify/promotion/{reference}")]
        //public async Task<ActionResult> Verify(string reference, [FromBody] Promotion promotion)
        //{
        //    var transaction = await _paymentService.GetTransactionByRefAsync(reference);
        //    if (transaction.ProfileId != _profile.Identifier)
        //        return Unauthorized(new DataResponse
        //        {
        //            Data = 403,
        //            Status = "Access dienied to the requested resource"
        //        });

        //    TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
        //    if (response.Data.Status == "success")
        //    {
        //        var verifyStatus = await _paymentService.VerifyTransactionAsync(transaction);
        //        if (verifyStatus != null && verifyStatus.Status)
        //        {
        //            var promotionOk = await _postsService.PromoteAsync(_profile, promotion);
        //            if (promotionOk)
        //                return Ok(new BasicResponse
        //                {
        //                    Success = true,
        //                    Message = response.Message
        //                });
        //        }
        //    }
        //    return BadRequest(new BasicResponse { Message = "Error verifying paid promotion" });
        //}

        [HttpPost("verify/accept_gift/{id}/{reference}")]
        public async Task<ActionResult<BasicResponse>> VerifyAcceptGift(Guid id, string reference)
        {
            TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
            if (response.Data.Status == "success")
            {
                var transaction = new Transaction
                {
                    Amount = 1500,
                    ProfileId = _profile.Identifier,
                    CreatedAt = DateTime.Now,
                    TrxRef = response.Data.Reference,
                    ItemId = id
                };

                var resultOk = await _paymentService.InsertTransactionAsync(transaction);
                if (resultOk)
                {
                    var acceptResultOk = await _postsService.AcceptGift(id);
                    if (acceptResultOk)
                        return Ok(new BasicResponse { Success = true, Message = response.Message });
                }
            }
            return BadRequest(new BasicResponse { Message = "Error verifying payment" });
        }

        //[HttpPost("verify/top_up/{reference}")]
        //public async Task<ActionResult> VerifyTopUpBalance(string reference)
        //{
        //    var transaction = await _paymentService.GetTransactionByRefAsync(reference);
        //    if (transaction.ProfileId != _profile.Identifier)
        //        return Unauthorized(new DataResponse
        //        {
        //            Data = 403,
        //            Status = "Access dienied to the requested resource"
        //        });
        //    TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
        //    if (response.Data.Status == "success")
        //    {
        //        var verifyStatus = await _paymentService.VerifyTransactionAsync(transaction);
        //        if (verifyStatus != null && verifyStatus.Status)
        //        {
        //            var resultOk = await _paymentService.InsertTransactionAsync(verifyStatus);
        //            if (resultOk)
        //                return Ok(new BasicResponse { Success = true, Message = response.Message });
        //        }
        //    }
        //    return BadRequest(new BasicResponse { Message = "Error verifying payment" });
        //}

        //[HttpPost("verify/purchase/gift/{reference}")]
        //public async Task<ActionResult> VerifyGiftPurchase(string reference)
        //{
        //    var transaction = await _paymentService.GetTransactionByRefAsync(reference);
        //    if (transaction.ProfileId != _profile.Identifier)
        //        return Unauthorized(new DataResponse
        //        {
        //            Data = 403,
        //            Status = "Access dienied to the requested resource"
        //        });
        //    TransactionVerifyResponse response = PayStack.Transactions.Verify(reference);
        //    if (response.Data.Status == "success")
        //    {
        //        var count = response.Data.Amount / 50000;
        //        var verifyStatus = await _paymentService.VerifyTransactionAsync(transaction);
        //        if (verifyStatus != null && verifyStatus.Status)
        //        {
        //            var giftsBought = await _paymentService.BuyGiftsAsync(_profile.Identifier, count);
        //            if (giftsBought)
        //                return Ok(new BasicResponse { Success = true, Message = response.Message });
        //        }
        //    }
        //    return BadRequest(new BasicResponse { Message = "Error verifying payment" });
        //}

        private static int Generate()
        {
            Random r = new((int)DateTime.Now.Ticks);
            return r.Next(100000000, 999999999);
        }
    }
}
