using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ProTrendAPI.Services.Network;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace ProTrendAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IConfiguration _configuration;
        public AuthenticationController(IConfiguration configuartion, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuartion;
        }

        [HttpGet]
        [CookieAuthenticationFilter]
        public ActionResult<Profile> GetMe()
        {
            if (_profile == null)
                return Unauthorized(new ErrorDetails { StatusCode = 401, Message = "User is UnAuthorized" });
            return Ok(_profile);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Register(ProfileDTO request)
        {
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { Success = false, Message = Constants.InvalidEmail });
            }

            if (request.UserName.Contains(' '))
            {
                return BadRequest(new { Success = false, Message = "Username cannot contain whitespace" });
            }

            var userExists = await GetUserResult(request);

            if (userExists != null)
            {
                return BadRequest(new { Success = false, Message = Constants.UserExists });
            }

            //Please modify before launching
            // var otp = SendEmail(request.Email, request.Password);
            return Ok(new { Success = true, OTP = GenerateOTP() });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!IsValidEmail(email))
                return BadRequest(new { Success = false, Message = Constants.InvalidEmail });

            var userExists = await GetUserResult(new ProfileDTO { Email = email });
            if (userExists == null)
            {
                return BadRequest(new { Success = false, Message = Constants.UserNotFound });
            }
            //SendEmail(email)
            return Ok();
        }

        [HttpPut("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ProfileDTO profile)
        {
            if (!IsValidEmail(profile.Email))
                return BadRequest(new { Success = false, Message = Constants.InvalidEmail });
            var register = await GetUserResult(new ProfileDTO { Email = profile.Email });
            if (register == null)
                return BadRequest(new { Success = false, Message = Constants.UserNotFound });

            CreatePasswordHash(profile.Password, out byte[] passwordHash, out byte[] passwordSalt);
            register.PasswordHash = passwordHash;
            register.PasswordSalt = passwordSalt;
            var result = await _regService.ResetPassword(register);
            if (result == null)
                return BadRequest(new { Success = false, Message = "Error occurred when resetting password, please try again!" });
            return Ok(new { Success = true, Message = "Password reset" });
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
        public async Task<ActionResult<object>> VerifyOTP(ProfileDTO request)
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
                return BadRequest(new { Success = false, Message = "Error when registering user!" });
            var authenticated = CreateToken(register);
            if (authenticated)
                return Ok(new { Success = true, Message = "OTP verified" });
            return BadRequest(new { Success = false, Message = "Verification failed" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Login(Login login)
        {
            if (!IsValidEmail(login.Email))
            {
                return BadRequest(new { Success = false, Message = Constants.InvalidEmail });
            }

            var result = await _regService.FindRegisteredUserByEmailAsync(login);

            if (result == null)
                return BadRequest(new { Success = false, Message = Constants.UserNotFound });
            if (!VerifyPasswordHash(result, login.Password, result.PasswordHash))
                return BadRequest(new { Success = false, Message = Constants.WrongEmailPassword });

            if (CreateToken(result))
            {
                return Ok(new { Success = true, Message = "Login success!" });
            }
            return BadRequest(new { Success = false, Message = "Login failed!" });
        }

        [HttpPost("logout")]
        [CookieAuthenticationFilter]
        public ActionResult<object> Logout()
        {
            try
            {
                HttpContext.Response.Cookies.Delete(Constants.AUTH);
                return Ok(new { Success = true, Message = "Logout successful" });
            }
            catch (Exception)
            {
                return BadRequest(new { Success = false, Message = "Logout failed" });
            }
        }

        private async Task<Register?> GetUserResult(ProfileDTO request)
        {
            return await _regService.FindRegisteredUserAsync(request);
        }

        private bool CreateToken(Register user)
        {
            try
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

                var sk = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[Constants.TokenLoc]));
                var credentials = new SigningCredentials(sk, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddHours(1), signingCredentials: credentials);
                var tokenResult = new JwtSecurityTokenHandler().WriteToken(token);
                var cookieOptions = new CookieOptions
                {
                    IsEssential = true,
                    Expires = DateTimeOffset.Now.AddMinutes(30),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append(Constants.AUTH, tokenResult, cookieOptions);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
