using System.Linq;
using SITConnect.Models;

namespace SITConnect.Services
{
    public class AuditLogService
    {
        private readonly AuditLogDbContext _context;

        public AuditLogService(AuditLogDbContext context)
        {
            _context = context;
        }

        public void AddLog(AuditLog userObject)
        {
            // if (_context.Audits.Any(o => o.audit == userObject.Id)) return false;
            _context.Audits.Add(userObject);
            _context.SaveChanges();
        }

        public IQueryable<AuditLog> GetLogsByUserId(long userId)
        {
            var listOfLogs = _context.Audits.Where(o => o.ActorId == userId);
            return listOfLogs;
            // Reminder to self: .ToList() if it needs to be read. This returns an IQueryable to allow Linq to work.
        }
        
        public IQueryable<AuditLog> GetLogsByIp(string ipAddress)
        {
            var listOfLogs = _context.Audits.Where(o => o.IpAddress == ipAddress);
            return listOfLogs;
            // Reminder to self: .ToList() if it needs to be read. This returns an IQueryable to allow Linq to work.
        }
    }
}