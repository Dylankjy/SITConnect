using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SITConnect.Models
{
    public class UserDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public UserDbContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("MyConn");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}