using OrderManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Managers
{
    public static class UserManager
    {
        private static List<User> _users = new List<User>();
        private static int _nextUserId = 1;

        static UserManager()
        {
            // Add a default admin user for testing
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Username = "admin",
                Email = "admin@ordermaster.com",
                Password = "admin123",
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        public static bool RegisterUser(User user)
        {
            // Check if email already exists
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            user.UserId = _nextUserId++;
            _users.Add(user);
            return true;
        }

        public static User AuthenticateUser(string usernameOrEmail, string password)
        {
            var user = _users.FirstOrDefault(u =>
                (u.Username.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
                u.Password == password &&
                u.IsActive);

            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
            }

            return user;
        }

        public static List<User> GetAllUsers()
        {
            return _users.Where(u => u.IsActive).ToList();
        }

        public static User GetUserById(int userId)
        {
            return _users.FirstOrDefault(u => u.UserId == userId && u.IsActive);
        }

        public static bool UpdateUser(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                return true;
            }
            return false;
        }

        public static void ClearUsers()
        {
            _users.Clear();
            _nextUserId = 1;
        }
    }
}
