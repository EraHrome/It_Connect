using ItConnectionBotApiCore.Models.DbModels;
using ItConnectionBotApiCore.Models;
using ItConnectionBotApiCore.Enums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ItConnectionBotApiCore.Providers
{
    public class EntityItTaskProvider
    {

        private readonly string _connectionString = @"Server=tcp:itconnectuserdb.database.windows.net,1433;Initial Catalog=ItConnectionDb;Persist Security Info=False;User ID={your_id};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static Context _db;

        public EntityItTaskProvider()
        {
            _db = new Context(_connectionString);
        }

        public void AddTask(Offer offers)
        {
            _db.Add(offers);
            _db.SaveChanges();
        }

        public void DeleteTask(Offer itTasks)
        {
            _db.Remove(itTasks);
            _db.SaveChanges();
        }

        public IEnumerable<Offer> GetAllMyCreatedTasks(string id) => _db.Offers.Where(x => x.CreatedUserId == id);
        public IEnumerable<Offer> GetAllMyOffers(string id) => _db.Offers.Where(x => x.Id.ToString() == id);
        public IEnumerable<Offer> GetAllNewTasks() => _db.Offers.Where(x => x.ItTaskStatus == ItTaskStatusEnum.New);
        public IEnumerable<Offer> GetAllInWorkTasks() => _db.Offers.Where(x => x.ItTaskStatus == ItTaskStatusEnum.InWork);
        public IEnumerable<Offer> GetAllExpiredTasks() => _db.Offers.Where(x => x.ItTaskStatus == ItTaskStatusEnum.Expired);
        public IEnumerable<Offer> GetAllReadyTasks() => _db.Offers.Where(x => x.ItTaskStatus == ItTaskStatusEnum.Ready);
        public IEnumerable<Offer> GetAllRemovedTasks() => _db.Offers.Where(x => x.ItTaskStatus == ItTaskStatusEnum.Removed);

    }
}
