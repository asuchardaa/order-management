using OrderManagement.Managers;
using OrderManagement.Model;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderManagement.UI.Windows
{
    public partial class SignupWindow : Window
    {
        private bool _isClosing = false;

        public SignupWindow()
        {
            InitializeComponent();
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;

            if (ValidateForm())
            {
                var newUser = new User
                {
                    FullName = GetTextBoxValue(txtFullName),
                    Email = GetTextBoxValue(txtEmail),
                    Username = GetTextBoxValue(txtEmail), // Using email as username
                    Password = txtPassword.Password,
                    Role = "Customer",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                if (UserManager.RegisterUser(newUser))
                {
                    MessageBox.Show("Účet byl úspěšně vytvořen! Nyní se můžete přihlásit.",
                        "Registrace úspěšná", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Návrat na login window
                    ReturnToLogin();
                }
                else
                {
                    MessageBox.Show("Účet s tímto emailem již existuje.",
                        "Registrace selhala", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ReturnToLogin()
        {
            if (!_isClosing)
            {
                _isClosing = true;

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                SafeClose();
            }
        }

        private string GetTextBoxValue(Xceed.Wpf.Toolkit.WatermarkTextBox textBox)
        {
            return string.IsNullOrWhiteSpace(textBox.Text) ? "" : textBox.Text;
        }

        private bool ValidateForm()
        {
            string fullName = GetTextBoxValue(txtFullName);
            string email = GetTextBoxValue(txtEmail);
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Validate full name
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Zadejte prosím své celé jméno.");
                txtFullName.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Zadejte prosím svou emailovou adresu.");
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Zadejte prosím platnou emailovou adresu.");
                txtEmail.Focus();
                return false;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Zadejte prosím heslo.");
                txtPassword.Focus();
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Heslo musí mít alespoň 6 znaků.");
                txtPassword.Focus();
                return false;
            }

            // Validate password confirmation
            if (password != confirmPassword)
            {
                ShowError("Hesla se neshodují.");
                txtConfirmPassword.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            if (!_isClosing)
            {
                MessageBox.Show(message, "Chyba při registraci", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing) {
                this.Hide();
                var loginWindow = new LoginWindow();
                bool? result = loginWindow.ShowDialog();

                if (!_isClosing) {
                    if (result != true)
                    {
                        this.Show();
                    }
                    else
                    {
                        SafeClose();
                    }
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

        private void SocialLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;

            var button = sender as Button;
            string provider = button?.Content.ToString();

            MessageBox.Show($"Registrace přes {provider} zatím není implementována.",
                "Funkce není dostupná", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_isClosing)
            {
                CreateAccount_Click(sender, e);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _isClosing = true;

            // Pokud DialogResult není nastavený, znamená to zrušení (X button)
            if (this.DialogResult == null)
            {
                this.DialogResult = false;
            }

            base.OnClosing(e);
        }
    }
}