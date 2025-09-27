using OrderManagement.Managers;
using OrderManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrderManagement.UI.Windows
{
    public partial class SignupWindow : Window
    {
        public SignupWindow()
        {
            InitializeComponent();
            SetupPlaceholders();
        }

        private void SetupPlaceholders()
        {
            txtFullName.GotFocus += RemovePlaceholder;
            txtFullName.LostFocus += AddPlaceholder;
            txtEmail.GotFocus += RemovePlaceholder;
            txtEmail.LostFocus += AddPlaceholder;
            AddPlaceholder(txtFullName, null);
            AddPlaceholder(txtEmail, null);
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
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    string placeholder = "";
                    if (textBox.Name == "txtFullName")
                        placeholder = "Your full name";
                    else if (textBox.Name == "txtEmail")
                        placeholder = "Your email address";

                    textBox.Text = placeholder;
                    textBox.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#888888");
                }
            }
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var newUser = new User
                {
                    FullName = GetTextBoxValue(txtFullName),
                    Email = GetTextBoxValue(txtEmail),
                    Username = GetTextBoxValue(txtEmail),
                    Password = txtPassword.Password,
                    Role = "Customer",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                if (UserManager.RegisterUser(newUser))
                {
                    MessageBox.Show("Account created successfully! You can now log in.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An account with this email already exists.",
                        "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            if (textBox.Foreground.ToString() == "#FF888888")
                return "";
            return textBox.Text;
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
                ShowError("Please enter your full name.");
                txtFullName.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email address.");
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                txtEmail.Focus();
                return false;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter a password.");
                txtPassword.Focus();
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                txtPassword.Focus();
                return false;
            }

            // Validate password confirmation
            if (password != confirmPassword)
            {
                ShowError("Passwords do not match.");
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
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void SocialLogin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string provider = button?.Content.ToString();

            MessageBox.Show($"{provider} login is not implemented yet.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Handle Enter key press for form submission
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CreateAccount_Click(sender, e);
            }
        }
    }
}