using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using ProTrendAPI.Models.User;
using System.Text.RegularExpressions;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RegistrationService _regService;
        private readonly IUserService _userService;

        public AuthenticationController(IConfiguration configuration, RegistrationService regService, IUserService userService)
        {
            _configuration = configuration;
            _regService = regService;
            _userService = userService;
        }

        [HttpGet, Authorize]
        public ActionResult<DataResponse> GetMe()
        {
            return Ok(new DataResponse { Data = _userService.GetProfile() });
        }

        [HttpPost("register")]
        public async Task<ActionResult<BasicResponse>> Register(ProfileDTO request)
        {
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });
            }

            if(request.UserName.Contains(' '))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = "User Name cannot contain whitespace" });
            }

            var userExists = await GetUserResult(request);

            if (userExists != null)
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserExists });
            }

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

            await _regService.InsertAsync(register);
            return Ok(new TokenResponse { Token = CreateToken(register) });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(ProfileDTO request)
        {
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.InvalidEmail });
            }

            var result = await GetUserResult(request);
            
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            if (result.AccountType == Constants.Disabled)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.AccountDisabled });
            if (!VerifyPasswordHash(result, request.Password, result.PasswordHash))
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.WrongEmailPassword });
            
            return Ok(new TokenResponse { Token = CreateToken(result) });
        }

        private async Task<Register?> GetUserResult(ProfileDTO request)
        {
            return await _regService.FindRegisteredUserAsync(request);
        }

        private string CreateToken(Register user)
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

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection(Constants.TokenLoc).Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
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
    }
}
