using ItConnectionBotApiCore.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace ItConnectionBotApiCore.Models
{
    public class Context : DbContext
    {

        private readonly string _connectionString;
        public DbSet<User> Users { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<InDesireOfferInfo> InDesireOffers { get; set; }

        public Context(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }

}
