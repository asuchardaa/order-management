using System;
using System.Collections.Generic;
using System.IO;
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
        // Profile picture
        public string ProfilePicture { get; set; }
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

        public string GetProfileImagePath()
        {
            if (string.IsNullOrEmpty(ProfilePicture))
                return null;

            // Pokud je to Base64, vrátit přímo
            if (ProfilePicture.StartsWith("data:image"))
                return ProfilePicture;

            // Pokud je to relativní cesta, převést na absolutní
            if (!Path.IsPathRooted(ProfilePicture))
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OrderMaster");
                return Path.Combine(appDataPath, ProfilePicture);
            }

            return ProfilePicture;
        }

        // Metoda pro nastavení profilového obrázku
        public void SetProfilePicture(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                // Můžete zkopírovat soubor do app složky
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "OrderMaster", "Profiles");

                Directory.CreateDirectory(appDataPath);

                string fileName = $"user_{UserId}_{Path.GetFileName(imagePath)}";
                string destPath = Path.Combine(appDataPath, fileName);

                File.Copy(imagePath, destPath, true);
                ProfilePicture = $"Profiles/{fileName}";
            }
        }
    }
}
