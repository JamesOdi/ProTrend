using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using ProTrendAPI.Services.Network;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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
        [AllowAnonymous]
        public ActionResult<Profile> GetMe()
        {
            if (_profile == null)
                return Unauthorized(new ActionResponse { StatusCode = 401, Message = ActionResponseMessage.Unauthorized });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = _profile });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Register(ProfileDTO request)
        {
            var userExists = await GetUserResult(request);

            if (userExists != null)
            {
                return Conflict(new ActionResponse { Successful = false, StatusCode = 409, Message = ActionResponseMessage.Conflict });
            }

            // Please modify before launching
            // var otp = SendEmail(request.Email, request.Password);
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = GenerateOTP() });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!IsValidEmail(email))
                return BadRequest(new ActionResponse { Message = $"{ActionResponseMessage.BadRequest}, invalid email address" });

            var userExists = await GetUserResult(new ProfileDTO { Email = email });
            if (userExists == null)
            {
                return BadRequest(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });
            }
            //SendEmail(email)
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = email });
        }

        [HttpPut("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ProfileDTO profile)
        {
            if (!IsValidEmail(profile.Email))
                return BadRequest(new ActionResponse { Message = $"{ActionResponseMessage.BadRequest}, invalid email address" });
            var register = await GetUserResult(new ProfileDTO { Email = profile.Email });
            if (register == null)
                return BadRequest(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound });

            CreatePasswordHash(profile.Password, out byte[] passwordHash, out byte[] passwordSalt);
            register.PasswordHash = passwordHash;
            register.PasswordSalt = passwordSalt;
            var result = await _regService.ResetPassword(register);
            if (result == null)
                return BadRequest(new ActionResponse { Message = ActionResponseMessage.BadRequest });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok });
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
                return BadRequest(new ActionResponse { Message = ActionResponseMessage.BadRequest });
            var cypherText = CreateToken(register);
            if (cypherText != "")
            {
                return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = cypherText });
            }
            return BadRequest(new ActionResponse { Message = ActionResponseMessage.BadRequest });
        }

        [HttpPost("mobile/verify/otp")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> MobileVerifyOTP(ProfileDTO request)
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
                return BadRequest(new ActionResponse { Message = ActionResponseMessage.BadRequest });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = result });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Login([FromBody] Login login)
        {
            if (!IsValidEmail(login.Email))
            {
                return BadRequest(new ActionResponse { Message = Constants.InvalidEmail });
            }

            var result = await _regService.FindRegisteredUserByEmailAsync(login);

            if (result == null)
                return BadRequest(new ActionResponse { StatusCode = 404, Message = Constants.UserNotFound });
            if (!VerifyPasswordHash(result, login.Password, result.PasswordHash))
                return BadRequest(new ActionResponse { Message = Constants.WrongEmailPassword });
            var cypherText = CreateToken(result);
            if (cypherText != "")
            {
                return Ok(new { Success = true, Data = cypherText });
            }
            return BadRequest(new ActionResponse { Message = ActionResponseMessage.BadRequest });
        }

        [HttpPost("mobile/login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> LoginMobile([FromBody] Login login)
        {
            if (!IsValidEmail(login.Email))
            {
                return BadRequest(new ActionResponse { Message = Constants.InvalidEmail, Data = new Profile() });
            }

            var result = await _regService.FindRegisteredUserByEmailAsync(login);

            if (result == null)
                return BadRequest(new ActionResponse { StatusCode = 404, Message = ActionResponseMessage.NotFound, Data = new Profile() });
            if (!VerifyPasswordHash(result, login.Password, result.PasswordHash))
                return BadRequest(new ActionResponse { Data = new Profile() });
            var token = GetJWT(result);
            
            if (token != "")
            {
                Response.Headers.Add("Authorization", token);
                return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok, Data = await _profileService.GetProfileByIdAsync(result.Id)});
            }
            return BadRequest(new ActionResponse { Data = new Profile() });
        }

        [HttpPost("logout")]
        [CookieAuthenticationFilter]
        public ActionResult<object> Logout()
        {
            try
            {
                HttpContext.Response.Cookies.Delete(Constants.AUTH);
                return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.Ok });
            }
            catch (Exception)
            {
                return BadRequest(new ActionResponse { Message = "Logout failed" });
            }
        }

        private async Task<Register?> GetUserResult(ProfileDTO request)
        {
            return await _regService.FindRegisteredUserAsync(request);
        }

        private string CreateToken(Register user)
        {
            try
            {
                return EncryptDataWithAes(GetJWT(user));
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string GetJWT(Register user)
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
            return new JwtSecurityTokenHandler().WriteToken(token);
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

        private string EncryptDataWithAes(string plainText)
        {
            byte[] inputArray = Encoding.UTF8.GetBytes(plainText);
            var tripleDES = Aes.Create();
            tripleDES.Key = Encoding.UTF8.GetBytes(_configuration["Token:SecretKey"]);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
    }
}
