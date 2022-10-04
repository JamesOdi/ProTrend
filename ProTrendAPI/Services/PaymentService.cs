using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Settings;


namespace ProTrendAPI.Services
{
    public class PaymentService: BaseService
    {
        public PaymentService(IOptions<DBSettings> settings):base(settings)
        {
                
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

            var transaction = new Transaction { Amount = -total, CreatedAt = DateTime.Now, ProfileId = profile.Identifier, Status = true, TrxRef = Generate().ToString() };
            await _transactionCollection.InsertOneAsync(transaction);

            //var companyBody = $"Reqest for withdrawal of N{total} from {profile.Email}";
            //SendMail("maryse.abshire24@ethereal.email", companyBody);
            //var senderBody = $"Your reqest for withdrawal of <b>{total} Gifts</b> is being processed. Your withdrawal will be sent to you within <b>24hrs</b>. Thank you. <p>If you face any challenges please send an email to customer support with request ID {transaction.Identifier} and we will get back to you as soon as we can</p>";
            //SendMail(profile.Email, senderBody);
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
