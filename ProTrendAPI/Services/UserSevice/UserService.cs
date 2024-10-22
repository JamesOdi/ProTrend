﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace ProTrendAPI.Services.UserSevice
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;

        public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _contextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public Profile? GetProfile()
        {
            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                try
                {
                    var result = new Profile();
                    var claim = _contextAccessor.HttpContext.User;
                    result.Email = claim.Claims.First(x => x.Type == Constants.Email).Value;
                    result.Id = Guid.Parse(claim.Claims.First(x => x.Type == Constants.ID).Value);
                    result.Identifier = Guid.Parse(claim.Claims.First(x => x.Type == Constants.Identifier).Value);
                    result.UserName = claim.Claims.First(x => x.Type == Constants.Name).Value;
                    result.FullName = claim.Claims.First(x => x.Type == Constants.FullName).Value;
                    result.AccountType = claim.Claims.First(x => x.Type == Constants.AccType).Value;
                    result.Country = claim.Claims.First(x => x.Type == Constants.Country).Value;
                    result.Disabled = bool.Parse(claim.Claims.First(x => x.Type == Constants.Disabled).Value);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        private string DecryptDataWithAes(byte[] cipherText)
        {
            var tripleDES = Aes.Create();
            tripleDES.Key = Encoding.UTF8.GetBytes(_configuration["Token:SecretKey"]);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
            tripleDES.Clear();
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}