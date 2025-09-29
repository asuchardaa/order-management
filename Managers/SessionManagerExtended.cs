using OrderManagement.Model;
using OrderManagement.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrderManagement.Managers
{
    public static class SessionManagerExtended
    {
        private static readonly TimeSpan DEFAULT_SESSION_TIMEOUT = TimeSpan.FromHours(8);
        private static System.Windows.Threading.DispatcherTimer _sessionTimer;
        private static List<UserActivity> _userActivities = new List<UserActivity>();

        static SessionManagerExtended()
        {
            InitializeSessionTimer();
        }

        private static void InitializeSessionTimer()
        {
            _sessionTimer = new System.Windows.Threading.DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromMinutes(1); // Kontrola každou minutu
            _sessionTimer.Tick += SessionTimer_Tick;
        }

        private static void SessionTimer_Tick(object sender, EventArgs e)
        {
            if (SessionManager.IsLoggedIn && IsSessionExpired())
            {
                HandleSessionExpiration();
            }
        }

        public static void StartSession(User user)
        {
            SessionManager.CurrentUser = user;
            _sessionTimer.Start();
            LogActivity("Session started");
        }

        public static void EndSession()
        {
            LogActivity("Session ended");
            SessionManager.Logout();
            _sessionTimer.Stop();

            // Vymazání citlivých dat z paměti
            ClearSensitiveData();
        }

        public static void ExtendSession()
        {
            if (SessionManager.IsLoggedIn)
            {
                // Obnovení času přihlášení
                typeof(SessionManager).GetField("_loginTime",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, DateTime.Now);

                LogActivity("Session extended");
            }
        }

        public static bool IsSessionExpired()
        {
            return SessionManager.SessionDuration > DEFAULT_SESSION_TIMEOUT;
        }

        public static void LogActivity(string activity)
        {
            if (SessionManager.IsLoggedIn)
            {
                var userActivity = new UserActivity
                {
                    UserId = SessionManager.CurrentUser.UserId,
                    Activity = activity,
                    Timestamp = DateTime.Now,
                    UserName = SessionManager.CurrentUser.FullName
                };

                _userActivities.Add(userActivity);

                // Omezení počtu uložených aktivit
                if (_userActivities.Count > 1000)
                {
                    _userActivities.RemoveRange(0, 500);
                }

                System.Diagnostics.Debug.WriteLine($"[{userActivity.Timestamp}] {userActivity.UserName}: {activity}");
            }
        }

        public static List<UserActivity> GetUserActivities(int? userId = null, int maxCount = 100)
        {
            var activities = _userActivities.AsQueryable();

            if (userId.HasValue)
            {
                activities = activities.Where(a => a.UserId == userId.Value);
            }

            return activities.OrderByDescending(a => a.Timestamp)
                           .Take(maxCount)
                           .ToList();
        }

        public static void HandleSessionExpiration()
        {
            LogActivity("Session expired");

            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(
                    "Vaše relace vypršela z důvodu nečinnosti. Chcete se znovu přihlásit?",
                    "Relace vypršela",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Přesměrování na přihlašovací stránku
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }

                // Zavření všech oken kromě přihlašovacího
                var windowsToClose = Application.Current.Windows.OfType<Window>()
                    .Where(w => !(w is LoginWindow)).ToList();

                foreach (var window in windowsToClose)
                {
                    window.Close();
                }
            });

            EndSession();
        }

        private static void ClearSensitiveData()
        {
            // Vymazání citlivých dat z paměti
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static TimeSpan GetRemainingSessionTime()
        {
            if (!SessionManager.IsLoggedIn)
                return TimeSpan.Zero;

            var remaining = DEFAULT_SESSION_TIMEOUT - SessionManager.SessionDuration;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public static void SetSessionTimeout(TimeSpan timeout)
        {
            // Možnost nastavení vlastního timeout
            typeof(SessionManagerExtended).GetField("DEFAULT_SESSION_TIMEOUT",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, timeout);
        }
    }
}
