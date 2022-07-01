using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using ProTrendAPI.Models.User;

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
            return Ok(new DataResponse { Data = _userService.GetUserProfile() });
        }

        [HttpPost("register")]
        public async Task<ActionResult<BasicResponse>> Register(UserDTO request)
        {
            var userExists = await GetUserResult(request);

            if (userExists != null)
            {
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserExists });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var register = new Register
            {
                Email = request.Email.ToLower(),
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                Name = request.Name.ToLower(),
                RegistrationDate = DateTime.Now,
                AccountType = request.AccountType,
                Country = request.Country!
            };

            await _regService.InsertAsync(register);
            return Ok(new BasicResponse { Message = Constants.Success });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            var result = await GetUserResult(request);
            
            if (result == null)
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.UserNotFound });
            if (!VerifyPasswordHash(result, request.Password, result.PasswordHash, result.PasswordSalt))
                return BadRequest(new BasicResponse { Status = Constants.Error, Message = Constants.WrongEmailPassword });
            
            return Ok(new TokenResponse { Token = CreateToken(result) });
        }

        private async Task<Register?> GetUserResult(UserDTO request)
        {
            return await _regService.FindRegisteredUserAsync(request);
        }

        private string CreateToken(Register user)
        {
            List<Claim> claims = new()
            {
                new Claim(Constants.ID, user.Id.ToString()),
                new Claim(Constants.Identifier, user.Id.ToString()),
                new Claim(Constants.Name, user.Name),
                new Claim(Constants.Email, user.Email),
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

        private static bool VerifyPasswordHash(Register user, string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(passwordHash);
        }
    }
}
