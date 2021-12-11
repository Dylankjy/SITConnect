using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SITConnect.Models
{
    public class AuditLogDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public AuditLogDbContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        public DbSet<AuditLog> Audits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _config.GetConnectionString("MyConn");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}