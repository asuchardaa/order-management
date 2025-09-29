using OrderManagement.UI.Windows;
using System;
using System.Windows;

namespace OrderManagement
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Nastavení exception handlerů
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Spuštění aplikace s LoginWindow místo MainWindow
            try
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při spuštění aplikace: {ex.Message}",
                    "Kritická chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Neočekávaná chyba: {e.Exception.Message}\n\nAplikace bude ukončena.",
                "Chyba aplikace", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Environment.Exit(1);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show($"Kritická chyba: {ex?.Message ?? "Neznámá chyba"}\n\nAplikace bude ukončena.",
                "Kritická chyba", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup při ukončení aplikace
            try
            {
                // Zde můžete přidat cleanup kód
                if (SessionManager.IsLoggedIn)
                {
                    SessionManager.Logout();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při ukončování: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}