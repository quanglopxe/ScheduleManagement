using QL_LICHHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class AccountRepository
    {
        private QL_LICHHOPDataContext _context;
        public AccountRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        public User Login(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == username && u.PasswordHash == password);
        }
    }
}