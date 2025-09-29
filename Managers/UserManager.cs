using OrderManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace OrderManagement.Managers
{
    public static class UserManager
    {
        private static List<User> _users = new List<User>();
        private static int _nextUserId = 1;
        private const string USERS_FILE = "users.json";

        static UserManager()
        {
            LoadUsers();

            // Pokud nejsou žádní uživatelé, vytvoř výchozí admin účet
            if (!_users.Any())
            {
                CreateDefaultUsers();
            }
        }

        private static void CreateDefaultUsers()
        {
            // Default admin user
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Username = "admin",
                Email = "admin@ordermaster.com",
                Password = "admin123",
                FullName = "Systémový Administrátor",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            // Default manager user
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Username = "manager",
                Email = "manager@ordermaster.com",
                Password = "manager123",
                FullName = "Jan Novák",
                Role = "Manager",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            // Default customer user
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Username = "customer",
                Email = "customer@ordermaster.com",
                Password = "customer123",
                FullName = "Marie Svobodová",
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            SaveUsers();
        }

        private static void LoadUsers()
        {
            try
            {
                if (File.Exists(USERS_FILE))
                {
                    string jsonString = File.ReadAllText(USERS_FILE);
                    var userData = JsonSerializer.Deserialize<UserData>(jsonString);

                    if (userData != null)
                    {
                        _users = userData.Users ?? new List<User>();
                        _nextUserId = userData.NextUserId;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při načítání uživatelů: {ex.Message}");
                _users = new List<User>();
                _nextUserId = 1;
            }
        }

        private static void SaveUsers()
        {
            try
            {
                var userData = new UserData
                {
                    Users = _users,
                    NextUserId = _nextUserId
                };

                string jsonString = JsonSerializer.Serialize(userData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(USERS_FILE, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při ukládání uživatelů: {ex.Message}");
            }
        }

        public static bool RegisterUser(User user)
        {
            // Check if email already exists
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Check if username already exists
            if (_users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            user.UserId = _nextUserId++;
            user.CreatedAt = DateTime.Now;
            _users.Add(user);
            SaveUsers();
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
                SaveUsers();
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
                existingUser.Username = user.Username;
                existingUser.Password = user.Password;
                existingUser.Role = user.Role;
                existingUser.LastLoginAt = user.LastLoginAt;
                existingUser.ProfilePicture = user.ProfilePicture;

                SaveUsers();
                return true;
            }
            return false;
        }

        public static bool DeactivateUser(int userId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.IsActive = false;
                SaveUsers();
                return true;
            }
            return false;
        }

        public static bool ChangePassword(int userId, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.Password = newPassword;
                SaveUsers();
                return true;
            }
            return false;
        }

        public static User FindUserByEmail(string email)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public static List<User> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllUsers();

            return _users.Where(u => u.IsActive &&
                (u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public static List<User> GetUsersByRole(string role)
        {
            return _users.Where(u => u.IsActive &&
                u.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static Dictionary<string, int> GetUserStatistics()
        {
            return new Dictionary<string, int>
            {
                ["TotalUsers"] = _users.Count(u => u.IsActive),
                ["Admins"] = _users.Count(u => u.IsActive && u.Role == "Admin"),
                ["Managers"] = _users.Count(u => u.IsActive && u.Role == "Manager"),
                ["Customers"] = _users.Count(u => u.IsActive && u.Role == "Customer"),
                ["NewUsersThisMonth"] = _users.Count(u => u.IsActive &&
                    u.CreatedAt.Month == DateTime.Now.Month &&
                    u.CreatedAt.Year == DateTime.Now.Year),
                ["ActiveToday"] = _users.Count(u => u.IsActive &&
                    u.LastLoginAt.HasValue &&
                    u.LastLoginAt.Value.Date == DateTime.Today)
            };
        }

        public static void ClearUsers()
        {
            _users.Clear();
            _nextUserId = 1;
            SaveUsers();
        }

        public static bool ValidatePassword(string password)
        {
            // Základní validace hesla
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 6)
                return false;

            return true;
        }

        public static string GenerateUsername(string fullName, string email)
        {
            // Generování uživatelského jména z plného jména nebo emailu
            string baseUsername;

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                baseUsername = fullName.Replace(" ", "").ToLower();
            }
            else
            {
                baseUsername = email.Split('@')[0].ToLower();
            }

            // Kontrola, zda uživatelské jméno již existuje
            string username = baseUsername;
            int counter = 1;

            while (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            return username;
        }
    }
    public class UserData
    {
        public List<User> Users { get; set; } = new List<User>();
        public int NextUserId { get; set; } = 1;
    }
}