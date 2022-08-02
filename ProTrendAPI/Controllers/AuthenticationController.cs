using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly RegistrationService _regService;
        private readonly IUserService _userService;

        public AuthenticationController(IConfiguration configuration, RegistrationService regService, IUserService userService)
        {
            _regService = regService;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<DataResponse> GetMe()
        {
            var profile = _userService.GetProfile();
            if (profile == null)
                throw new Exception();
            return Ok(new DataResponse { Data = profile });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<BasicResponse>> Register(ProfileDTO request)
        {
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });
            }

            if (request.UserName.Contains(' '))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = "Username cannot contain whitespace" });
            }

            var userExists = await GetUserResult(request);

            if (userExists != null)
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserExists });
            }

            //Please modify before launching
            // var otp = SendEmail(request.Email, request.Password);
            return Ok(new { Status = "success", OTP = GenerateOTP() });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!IsValidEmail(email))
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });

            var userExists = await GetUserResult(new ProfileDTO { Email = email });
            if (userExists == null)
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            }
            //SendEmail(email)
            return Ok();
        }

        [HttpPut("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ProfileDTO profile)
        {
            if (!IsValidEmail(profile.Email))
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });
            var register = await GetUserResult(new ProfileDTO { Email = profile.Email });
            if (register == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });

            CreatePasswordHash(profile.Password, out byte[] passwordHash, out byte[] passwordSalt);
            register.PasswordHash = passwordHash;
            register.PasswordSalt = passwordSalt;
            var result = await _regService.ResetPassword(register);
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = "Error occurred when resetting password, please try again!" });
            return Ok(new DataResponse { Status = "success", Data = true });
        }

        private static int SendEmail(string to, string password)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(to));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = "Your ProTrend One-Time-Password";
            var otp = GenerateOTP();
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = $"Your OTP is {otp}" };
            using var smtp = new SmtpClient();
            smtp.Connect(to, 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(to, password);
            smtp.Send(email);
            smtp.Disconnect(true);
            return otp;
        }

        [HttpPost("verify/otp")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> VerifyToken(ProfileDTO request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var register = new Register
            {
                Email = request.Email.Trim().ToLower(),
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                UserName = request.UserName.Trim().ToLower(),
                FullName = request.FullName.Trim().ToLower(),
                RegistrationDate = DateTime.Now,
                AccountType = request.AccountType.Trim().ToLower(),
                Country = request.Country!.Trim().ToLower()
            };
            var result = await _regService.InsertAsync(register);
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = "Error when registering user!" });
            CreateToken(register);
            return Ok(new DataResponse { Status = "success", Data = true });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableCors(Constants.CORS)]
        public async Task<ActionResult<object>> Login(Login request)
        {
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });
            }

            var result = await GetUserResult(new ProfileDTO { Email = request.Email, Password = request.Password });

            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            if (result.AccountType == Constants.Disabled)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.AccountDisabled });
            if (!VerifyPasswordHash(result, request.Password, result.PasswordHash))
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.WrongEmailPassword });
            CreateToken(result);
            return Ok(new DataResponse { Status = "success", Data = true });
        }

        [HttpPost("logout")]
        public async Task<ActionResult<object>> Logout()
        {
            await HttpContext.SignOutAsync("ProTrendAuth");
            return Ok(new DataResponse { Status = "success", Data = true });
        }

        private async Task<Register?> GetUserResult(ProfileDTO request)
        {
            return await _regService.FindRegisteredUserAsync(request);
        }

        private async void CreateToken(Register user)
        {
            List<Claim> claims = new()
            {
                new Claim(Constants.ID, user.Id.ToString()),
                new Claim(Constants.Identifier, user.Id.ToString()),
                new Claim(Constants.Name, user.UserName),
                new Claim(Constants.Email, user.Email),
                new Claim(Constants.FullName, user.FullName),
                new Claim(Constants.AccType, user.AccountType),
                new Claim(Constants.Country, user.Country),
            };

            bool disabled = false;
            if (user.AccountType == Constants.Disabled)
                disabled = true;

            claims.Add(new Claim(Constants.Disabled, disabled.ToString()));
            var identity = new ClaimsIdentity(claims, "ProTrendAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("ProTrendAuth", principal);
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(Register user, string password, byte[] passwordHash)
        {
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(passwordHash);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static int GenerateOTP()
        {
            var r = new Random();
            return r.Next(1000, 9999);
        }
    }
}
