﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Settings;
using System.Security.Cryptography;
using System.Text;

namespace ProTrendAPI.Services
{
    public class PaymentService: BaseService
    {
        public PaymentService(IOptions<DBSettings> settings):base(settings)
        {
                
        }

        private static string EncryptDataWithAes(string plainText, string token)
        {
            byte[] inputArray = Encoding.UTF8.GetBytes(plainText);
            var tripleDES = Aes.Create();
            tripleDES.Key = Encoding.UTF8.GetBytes(token);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public async Task<AccountDetails> AddAccountDetailsAsync(AccountDetailsDTO account, string token)
        {
            var accountDetails = new AccountDetails { CardNumber = EncryptDataWithAes(account.CardNumber, token), CVV = EncryptDataWithAes(account.CVV, token), ExpirtyDate = EncryptDataWithAes(account.ExpirtyDate, token), ProfileId = account.ProfileId };
            await _accountDetailsCollection.InsertOneAsync(accountDetails);
            return accountDetails;
        }

        public async Task<Transaction> GetTransactionByRefAsync(string reference)
        {
            return await _transactionCollection.Find(Builders<Transaction>.Filter.Eq(t => t.TrxRef, reference)).SingleOrDefaultAsync();
        }

        public async Task<bool> InsertTransactionAsync(Transaction transaction)
        {
            try
            {
                transaction.Id = Guid.NewGuid();
                await _transactionCollection.InsertOneAsync(transaction);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetTotalBalance(Guid profileId)
        {
            var filter = Builders<Transaction>.Filter.Where(s => s.ProfileId == profileId);
            var transactions = await _transactionCollection.Find(filter).ToListAsync();
            var total = 0;
            foreach (var transaction in transactions)
            {
                total += transaction.Amount;
            }
            return total;
        }

        public async Task<bool> RequestWithdrawalAsync(Profile profile, int total)
        {
            int balance = await GetTotalBalance(profile.Identifier);
            if (balance <= 100 || total < balance)
                return false;

            var transaction = new Transaction { Amount = -total, CreatedAt = DateTime.Now, ProfileId = profile.Identifier, Purpose = "Withdraw", TrxRef = Generate().ToString() };
            await _transactionCollection.InsertOneAsync(transaction);
            return true;
        }

        public async Task<int> GetTotalGiftsAsync(Guid profileId)
        {
            var gifts = await GetAllGiftAsync(profileId);
            return gifts.Count;
        }

        public async Task<List<Gift>> GetAllGiftAsync(Guid profileId)
        {
            var filter = Builders<Gift>.Filter.Where(s => s.ProfileId == profileId && s.Disabled == false);
            var gifts = await _giftsCollection.Find(filter).ToListAsync();
            return gifts;
        }

        public async Task<bool> BuyGiftsAsync(Guid profileId, int count)
        {
            var gifts = Enumerable.Repeat(new Gift { Id = null, ProfileId = profileId, Disabled = false }, count);
            try
            {
                await _giftsCollection.InsertManyAsync(gifts);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
