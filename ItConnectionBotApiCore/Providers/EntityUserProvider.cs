using ItConnectionBotApiCore.Models.DbModels;
using ItConnectionBotApiCore.Models;
using ItConnectionBotApiCore.Enums;
using System.Linq;
using System;

namespace ItConnectionBotApiCore.Providers
{
    public class EntityUserProvider
    {

        private readonly string _connectionString = @"Server=tcp:itconnectuserdb.database.windows.net,1433;Initial Catalog=ItConnectionDb;Persist Security Info=False;User ID={your_id};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static Context _db;

        public EntityUserProvider()
        {
            _db = new Context(_connectionString);
        }

        public void AddUser(User user)
        {
            _db.Add(user);
             _db.SaveChanges();
        }

        public void DeleteUser(User user)
        {
            _db.Remove(user);
            _db.SaveChanges();
        }

        public (ProviderResultEnum, User) AddUserIfNoExist(User user)
        {

            try
            {
                var foundedUser = GetUserByUserId(user.UserId);
                if (foundedUser == null)
                {
                    AddUser(user);
                    return (ProviderResultEnum.Success, user);
                }
                return (ProviderResultEnum.AlreadyExists, foundedUser);
            }
            catch (Exception ex)
            {
                return (ProviderResultEnum.Error, null);
            }

        }

        public User GetUserByUserId(string id) => _db.Users.FirstOrDefault(x => x.UserId == id);

        public void UpdateUser(User user) => _db.Update(user);

    }
}
