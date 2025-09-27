using OrderManagement.Managers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderManagement.UI.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            SetupPlaceholders();
        }

        private void SetupPlaceholders()
        {
            txtUsernameEmail.GotFocus += RemovePlaceholder;
            txtUsernameEmail.LostFocus += AddPlaceholder;
            AddPlaceholder(txtUsernameEmail, null);
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Foreground.ToString() == "#FF888888")
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.White;
                }
            }
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Email or username";
                textBox.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#888888");
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAndLogin()){}
        }

        private bool ValidateAndLogin()
        {
            string usernameOrEmail = GetTextBoxValue(txtUsernameEmail);
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                ShowError("Please enter your email or username.");
                txtUsernameEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return false;
            }

            var user = UserManager.AuthenticateUser(usernameOrEmail, password);

            if (user != null)
            {
                MessageBox.Show($"Welcome back, {user.FullName}!",
                    "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
                return true;
            }
            else
            {
                ShowError("Invalid email/username or password. Please try again.");
                txtPassword.Password = "";
                txtUsernameEmail.Focus();
                return false;
            }
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            if (textBox.Foreground.ToString() == "#FF888888") 
                return "";
            return textBox.Text;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var signupWindow = new SignupWindow();
            signupWindow.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Password reset functionality is not implemented yet.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SocialLogin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string provider = button?.Content.ToString();

            MessageBox.Show($"{provider} login is not implemented yet.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SignIn_Click(sender, e);
            }
        }
    }
}