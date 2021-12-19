using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SITConnect.Models
{
    public class OtpDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public OtpDbContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        public DbSet<OtpCode> OtpCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("MyConn");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}