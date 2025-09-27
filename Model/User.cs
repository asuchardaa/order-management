using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Model
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string GetInitials()
        {
            if (string.IsNullOrEmpty(FullName)) return "UN";

            var names = FullName.Split(' ');
            if (names.Length >= 2)
            {
                return $"{names[0][0]}{names[1][0]}".ToUpper();
            }
            return FullName.Substring(0, Math.Min(2, FullName.Length)).ToUpper();
        }
    }
}
