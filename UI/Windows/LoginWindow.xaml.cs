// Opravený LoginWindow.xaml.cs
using OrderManagement.Managers;
using OrderManagement.Model;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json;
using System.Linq;

namespace OrderManagement.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private const string CREDENTIALS_FILE = "user_credentials.dat";
        private const string AUTO_LOGIN_FILE = "auto_login.dat";
        private bool _isClosing = false;

        public LoginWindow()
        {
            InitializeComponent();
            LoadSavedCredentials();
            CheckAutoLogin();
        }

        private void LoadSavedCredentials()
        {
            try
            {
                if (File.Exists(CREDENTIALS_FILE))
                {
                    var encryptedData = File.ReadAllText(CREDENTIALS_FILE);
                    var credentials = DecryptCredentials(encryptedData);

                    if (credentials != null)
                    {
                        txtUsernameEmail.Text = credentials.Username;
                        txtPassword.Password = credentials.Password;
                        chkRememberMe.IsChecked = true;

                        // Automaticky se pokusit přihlásit, pokud jsou údaje uložené
                        if (credentials.AutoLogin)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!_isClosing)
                                {
                                    SignIn_Click(null, null);
                                }
                            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při načítání uložených údajů: {ex.Message}");
            }
        }

        private void CheckAutoLogin()
        {
            try
            {
                if (File.Exists(AUTO_LOGIN_FILE))
                {
                    var autoLoginData = File.ReadAllText(AUTO_LOGIN_FILE);
                    var loginInfo = JsonSerializer.Deserialize<AutoLoginInfo>(autoLoginData);

                    // Kontrola, zda není token příliš starý (7 dní)
                    if (loginInfo != null && DateTime.Now - loginInfo.SavedAt < TimeSpan.FromDays(7))
                    {
                        var user = UserManager.GetUserById(loginInfo.UserId);
                        if (user != null && !_isClosing)
                        {
                            // Automatické přihlášení
                            SessionManager.CurrentUser = user;
                            SessionManager.RememberMe = true;

                            ShowSuccessMessage($"Automaticky přihlášen jako {user.FullName}");

                            // Bezpečné vytvoření MainWindow
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!_isClosing)
                                {
                                    var mainWindow = new MainWindow(user);
                                    mainWindow.Show();
                                    SafeClose();
                                }
                            }));
                            return;
                        }
                    }
                    else
                    {
                        // Starý token - smazat
                        File.Delete(AUTO_LOGIN_FILE);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při automatickém přihlášení: {ex.Message}");
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing && ValidateAndLogin())
            {
                // Login successful - handled in ValidateAndLogin method
            }
        }

        private bool ValidateAndLogin()
        {
            if (_isClosing) return false;

            string usernameOrEmail = GetTextBoxValue(txtUsernameEmail);
            string password = txtPassword.Password;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                ShowError("Zadejte prosím email nebo uživatelské jméno.");
                txtUsernameEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Zadejte prosím heslo.");
                txtPassword.Focus();
                return false;
            }

            // Show loading state
            btnSignIn.Content = "Přihlašování...";
            btnSignIn.IsEnabled = false;

            try
            {
                // Attempt authentication
                var user = UserManager.AuthenticateUser(usernameOrEmail, password);

                if (user != null)
                {
                    // Login successful
                    SessionManager.CurrentUser = user;
                    SessionManager.RememberMe = chkRememberMe.IsChecked == true;

                    // Save credentials if remember me is checked
                    if (chkRememberMe.IsChecked == true)
                    {
                        SaveCredentials(usernameOrEmail, password, true);
                        SaveAutoLoginInfo(user.UserId);
                    }
                    else
                    {
                        // Clear saved credentials if remember me is unchecked
                        ClearSavedCredentials();
                    }

                    ShowSuccessMessage($"Vítejte zpět, {user.FullName}!");

                    // Update last login time
                    user.LastLoginAt = DateTime.Now;
                    UserManager.UpdateUser(user);

                    // Open main window with user context - bezpečně
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_isClosing)
                        {
                            var mainWindow = new MainWindow(user);
                            mainWindow.Show();
                            SafeClose();
                        }
                    }));

                    return true;
                }
                else
                {
                    ShowError("Neplatný email/uživatelské jméno nebo heslo. Zkuste to prosím znovu.");
                    txtPassword.Password = "";
                    txtUsernameEmail.Focus();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při přihlašování: {ex.Message}");
                return false;
            }
            finally
            {
                // Reset button state - pouze pokud okno není zavřené
                if (!_isClosing)
                {
                    btnSignIn.Content = "Přihlásit se";
                    btnSignIn.IsEnabled = true;
                }
            }
        }

        private void SafeClose()
        {
            if (!_isClosing)
            {
                _isClosing = true;
                try
                {
                    this.Close();
                }
                catch (InvalidOperationException)
                {
                    // Okno už je zavřené - ignorujeme
                }
            }
        }

        private void SaveCredentials(string username, string password, bool autoLogin)
        {
            try
            {
                var credentials = new SavedCredentials
                {
                    Username = username,
                    Password = password,
                    AutoLogin = autoLogin,
                    SavedAt = DateTime.Now
                };

                var encryptedData = EncryptCredentials(credentials);
                File.WriteAllText(CREDENTIALS_FILE, encryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při ukládání údajů: {ex.Message}");
            }
        }

        private void SaveAutoLoginInfo(int userId)
        {
            try
            {
                var autoLoginInfo = new AutoLoginInfo
                {
                    UserId = userId,
                    SavedAt = DateTime.Now,
                    Token = GenerateSecureToken()
                };

                var jsonData = JsonSerializer.Serialize(autoLoginInfo);
                File.WriteAllText(AUTO_LOGIN_FILE, jsonData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při ukládání auto-login: {ex.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                if (File.Exists(CREDENTIALS_FILE))
                    File.Delete(CREDENTIALS_FILE);

                if (File.Exists(AUTO_LOGIN_FILE))
                    File.Delete(AUTO_LOGIN_FILE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při mazání uložených údajů: {ex.Message}");
            }
        }

        private string EncryptCredentials(SavedCredentials credentials)
        {
            var json = JsonSerializer.Serialize(credentials);
            var data = Encoding.UTF8.GetBytes(json);

            // Simple encryption using machine key
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private SavedCredentials DecryptCredentials(string encryptedData)
        {
            try
            {
                var data = Convert.FromBase64String(encryptedData);
                var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(decrypted);

                return JsonSerializer.Deserialize<SavedCredentials>(json);
            }
            catch
            {
                return null;
            }
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        private string GetTextBoxValue(Xceed.Wpf.Toolkit.WatermarkTextBox textBox)
        {
            return string.IsNullOrWhiteSpace(textBox.Text) ? "" : textBox.Text;
        }

        private void ShowError(string message)
        {
            if (!_isClosing)
            {
                MessageBox.Show(message, "Chyba přihlášení", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowSuccessMessage(string message)
        {
            if (!_isClosing)
            {
                var originalTitle = this.Title;
                this.Title = message;

                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, e) =>
                {
                    if (!_isClosing)
                    {
                        this.Title = originalTitle;
                    }
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing)
            {
                // Skrýt LoginWindow místo zavření
                this.Hide();

                var signupWindow = new SignupWindow();
                bool? result = signupWindow.ShowDialog();

                // Po zavření SignupWindow:
                if (!_isClosing)
                {
                    // Pokud byla registrace úspěšná, zůstat skrytý
                    // Pokud byla zrušena, zobrazit znovu LoginWindow
                    if (result != true) // null nebo false = zrušeno
                    {
                        this.Show();
                    }
                    else
                    {
                        // Registrace byla úspěšná, zavřít LoginWindow
                        SafeClose();
                    }
                }
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing)
            {
                var forgotPasswordWindow = new ForgotPasswordWindow();
                forgotPasswordWindow.ShowDialog();
            }
        }

        private void SocialLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing)
            {
                var button = sender as Button;
                string provider = button?.Content.ToString();

                MessageBox.Show($"Přihlášení přes {provider} zatím není implementováno.",
                    "Funkce není dostupná", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_isClosing)
            {
                SignIn_Click(sender, e);
            }
        }

        private void RememberMe_Changed(object sender, RoutedEventArgs e)
        {
            if (chkRememberMe.IsChecked == false && !_isClosing)
            {
                ClearSavedCredentials();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _isClosing = true;

            // Pokud se uživatel neúspěšně přihlásil a zavřel okno, ukončíme aplikaci
            if (SessionManager.CurrentUser == null)
            {
                Application.Current.Shutdown();
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _isClosing = true;
            base.OnClosed(e);
        }
    }

    // Model třídy zůstávají stejné...
    public class SavedCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool AutoLogin { get; set; }
        public DateTime SavedAt { get; set; }
    }

    public class AutoLoginInfo
    {
        public int UserId { get; set; }
        public DateTime SavedAt { get; set; }
        public string Token { get; set; }
    }

// Rozšířený UserManager s dalšími metodami
public static class UserManagerExtensions
    {
        // Tyto metody už existují v UserManager, takže jen delegujeme
        public static User GetUserById(int userId)
        {
            return UserManager.GetUserById(userId);
        }

        public static bool UpdateUser(User user)
        {
            return UserManager.UpdateUser(user);
        }

        public static void UpdateLastLogin(int userId)
        {
            var user = UserManager.GetUserById(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
                UserManager.UpdateUser(user);
            }
        }

        public static bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = UserManager.GetUserById(userId);
            if (user != null && user.Password == oldPassword)
            {
                user.Password = newPassword;
                return UserManager.UpdateUser(user);
            }
            return false;
        }

        public static User FindUserByEmail(string email)
        {
            // Delegace na hlavní UserManager metodu
            return UserManager.FindUserByEmail(email);
        }

        // Dodatečné pomocné metody, které nejsou v hlavním UserManager
        public static bool IsEmailAvailable(string email)
        {
            return UserManager.FindUserByEmail(email) == null;
        }

        public static bool IsUsernameAvailable(string username)
        {
            var allUsers = UserManager.GetAllUsers();
            return !allUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public static User GetUserByUsername(string username)
        {
            var allUsers = UserManager.GetAllUsers();
            return allUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ValidateUserCredentials(string usernameOrEmail, string password)
        {
            return UserManager.AuthenticateUser(usernameOrEmail, password) != null;
        }

        public static int GetTotalUsersCount()
        {
            return UserManager.GetAllUsers().Count;
        }

        public static User[] GetRecentlyActiveUsers(int count = 10)
        {
            return UserManager.GetAllUsers()
                .Where(u => u.LastLoginAt.HasValue)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(count)
                .ToArray();
        }

        public static bool ResetPassword(string email, string newPassword)
        {
            var user = UserManager.FindUserByEmail(email);
            if (user != null)
            {
                user.Password = newPassword;
                return UserManager.UpdateUser(user);
            }
            return false;
        }
    }

    // Rozšířený SessionManager
    public static class SessionManagerExtensions
    {
        public static void ExtendSession()
        {
            if (SessionManager.IsLoggedIn)
            {
                // Prodloužit relaci - například obnovit auto-login token
                var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                // Implementovat logiku prodloužení relace
            }
        }

        public static bool IsSessionExpired(TimeSpan maxDuration)
        {
            return SessionManager.SessionDuration > maxDuration;
        }

        public static void LogActivity(string activity)
        {
            if (SessionManager.IsLoggedIn)
            {
                // Zaznamenat aktivitu uživatele
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] User {SessionManager.CurrentUser.FullName}: {activity}");
            }
        }
    }
}