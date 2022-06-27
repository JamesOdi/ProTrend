using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProTrendAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RegistrationService _dbService;
        public AuthenticationController(IConfiguration configuration, RegistrationService dBService)
        {
            _configuration = configuration;
            _dbService = dBService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserProfile>> Register(UserDTO request)
        {
            var userExists = await GetUserResult(request);
            if (userExists != null)
                return BadRequest("User already exists, please login!");
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var register = new Register
            {
                Email = request.Email,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                Name = request.Name,
                RegistrationDate = DateTime.Now,
                AccountType = request.AccountType,
                Country = request.Country!
            };
            // var token = CreateToken(register);
            return Ok(await _dbService.InsertAsync(register));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDTO request)
        {
            var result = await GetUserResult(request);
            var loginUser = result.Value;
            
            if (loginUser == null)
                return BadRequest("User not found!");
            if (!VerifyPasswordHash(loginUser, request.Password, loginUser.PasswordHash, loginUser.PasswordSalt))
                return BadRequest("Wrong email or password!");

            var token = CreateToken(loginUser);
            return Ok(token);
        }

        private async Task<ActionResult<Register?>> GetUserResult(UserDTO request)
        {
            return Ok(await _dbService.FindRegisteredUserAsync(request));
        }

        private string CreateToken(Register user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("_id", user.Id),
                new Claim("phash", user.PasswordHash.ToString()),
                new Claim("psalt", user.PasswordSalt.ToString()),
                new Claim(ClaimTypes.Anonymous, user.AccountType),
                new Claim("country", user.Country),
                new Claim("regDate", user.RegistrationDate.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
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
