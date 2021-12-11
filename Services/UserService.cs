using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SITConnect.Models;

namespace SITConnect.Services
{
    public class UserService
    {
        private readonly UserDbContext _context;

        public UserService(UserDbContext context)
        {
            _context = context;
        }

        public bool AddUser(User userObject)
        {
            if (_context.Users.Any(o => o.Id == userObject.Id)) return false;
            _context.Users.Add(userObject);
            _context.SaveChanges();

            return true;
        }

        public User GetUserById(long id)
        {
            var userObject = _context.Users.SingleOrDefault(o => o.Id == id);
            return userObject;
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                return _context.Users.SingleOrDefault(o => o.Email == email);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        // public bool CheckEmailExist(string email)
        // {
        //     Console.WriteLine(GetUserByEmail(email));
        //     if (GetUserByEmail(email) == null)
        //     {
        //         return false;
        //     }
        //
        //     return true;
        // }

        public bool DestroyUser(long id)
        {
            try
            {
                var userObject = GetUserById(id);
                _context.Remove(userObject);
                _context.SaveChanges();
                return true;
            }
            catch (DBConcurrencyException)
            {
                return false;
            }
        }

        public bool UpdateUser(User userObject)
        {
            _context.Attach(userObject).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}