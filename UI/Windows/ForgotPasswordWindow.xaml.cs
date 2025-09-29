using OrderManagement.Managers;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace OrderManagement.UI.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void SendReset_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Zadejte prosím emailovou adresu.");
                txtEmail.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Zadejte prosím platnou emailovou adresu.");
                txtEmail.Focus();
                return;
            }

            btnSendReset.Content = "Odesílání...";
            btnSendReset.IsEnabled = false;

            try
            {
                // Opraveno: Použití správné metody z UserManager
                var user = UserManager.FindUserByEmail(email);

                if (user != null)
                {
                    // Generování nového hesla
                    string newPassword = GenerateTemporaryPassword();

                    // Aktualizace hesla v systému
                    user.Password = newPassword;
                    UserManager.UpdateUser(user);

                    // Simulace odeslání emailu (v reálné aplikaci by se poslal skutečný email)
                    ShowPasswordResetDialog(user.FullName, newPassword);

                    this.Close();
                }
                else
                {
                    ShowError("Uživatel s touto emailovou adresou nebyl nalezen.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při obnovování hesla: {ex.Message}");
            }
            finally
            {
                btnSendReset.Content = "Odeslat nové heslo";
                btnSendReset.IsEnabled = true;
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new char[8];

            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        private void ShowPasswordResetDialog(string userName, string newPassword)
        {
            var message = $"Ahoj {userName}!\n\n" +
                         $"Vaše nové dočasné heslo je: {newPassword}\n\n" +
                         "Doporučujeme vám po přihlášení heslo změnit v nastavení profilu.\n\n" +
                         "V reálné aplikaci by vám bylo heslo odesláno emailem.";

            MessageBox.Show(message, "Heslo bylo obnoveno",
                MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show(message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}