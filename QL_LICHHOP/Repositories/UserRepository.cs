using QL_LICHHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class UserRepository
    {
        private QL_LICHHOPDataContext _context;
        public UserRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        public List<User> GetHosts()
        {
            return _context.Users.Where(u => u.RoleInMeet == true).ToList();
        }        
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public User GetUserByName(string fullName)
        {
            return _context.Users.FirstOrDefault(u => u.FullName == fullName);
        }
        public User AddNewHost(string fullName)
        {
            User user = new User
            {
                FullName = fullName,
                UserName = "guest" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                PasswordHash = "123",                
                Role = "Người dùng mới",
                DepartmentID = 6,
                RoleInMeet = true,
                CreatedAt = DateTime.Now
            };
            _context.Users.InsertOnSubmit(user);
            _context.SubmitChanges();
            return user;
        }
        public void SetUserToHost(User user)
        {
            user.RoleInMeet = true;
            _context.SubmitChanges();
        }
        public void SetUserToParticipant(User user)
        {
            user.RoleInMeet = false;
            _context.SubmitChanges();
        }
    }
}