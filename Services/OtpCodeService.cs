using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SITConnect.Models;

namespace SITConnect.Services
{
    public class OtpCodeService
    {
        private readonly OtpDbContext _context;

        public OtpCodeService(OtpDbContext context)
        {
            _context = context;
        }

        public bool AddCode(OtpCode otpObject)
        {
            if (_context.OtpCodes.Any(o => o.Code == otpObject.Code)) return false;
            _context.OtpCodes.Add(otpObject);
            _context.SaveChanges();

            return true;
        }

        public OtpCode GetCode(int code)
        {
            return _context.OtpCodes.SingleOrDefault(o => o.Code == code);
        }

        public bool DestroyAllCodesByUserId(long userId)
        {
            try
            {
                // This is bad practice but it shouldn't have any vulnerabilities to sql injection 
                // because this code doesn't take in any direct user input
                _context.Database.ExecuteSqlRaw($"delete from OtpCodes where UserId = {userId.ToString()}");
                return true;
            }
            catch (DBConcurrencyException)
            {
                return false;
            }
        }
    }
}