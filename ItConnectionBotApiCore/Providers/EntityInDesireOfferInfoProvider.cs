using ItConnectionBotApiCore.Models.DbModels;
using ItConnectionBotApiCore.Models;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ItConnectionBotApiCore.Providers
{
    public class EntityInDesireOfferInfoProvider
    {

        private readonly string _connectionString = @"Server=tcp:itconnectuserdb.database.windows.net,1433;Initial Catalog=ItConnectionDb;Persist Security Info=False;User ID={your_id};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static Context _db;

        public EntityInDesireOfferInfoProvider()
        {
            _db = new Context(_connectionString);
        }

        public bool MakeDesireOffer(int offerId, string desiredUserId)
        {
            var offer = GetOfferById(offerId);
            if (offer == null)
            {
                return false;
            }
            var desireOffer = new InDesireOfferInfo()
            {
                CreatedUserId = offer.CreatedUserId,
                InDesireUserId = desiredUserId,
                OfferId = offer.Id
            };
            _db.InDesireOffers.Add(desireOffer);
            _db.SaveChanges();
            return true;
        }

        public IEnumerable<InDesireOfferInfo> GetMyOffers(string chatId) => _db.InDesireOffers.Where(x => x.InDesireUserId == chatId);

        public Offer GetOfferById(int offerId) => _db.Offers.FirstOrDefault(x => x.Id == offerId);

    }
}
