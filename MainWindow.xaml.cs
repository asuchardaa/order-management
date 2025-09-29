using OrderManagement.Model;
using OrderManagement.Pages;
using OrderManagement.UI.Pages;
using OrderManagement.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace OrderManagement
{
    public static class VisualTreeHelper
    {
        public static int GetChildrenCount(DependencyObject parent)
        {
            return System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        }

        public static DependencyObject GetChild(DependencyObject parent, int childIndex)
        {
            return System.Windows.Media.VisualTreeHelper.GetChild(parent, childIndex);
        }
    }

    // Helper třída pro práci s Logical Tree
    public static class LogicalTreeHelper
    {
        public static System.Collections.IEnumerable GetChildren(DependencyObject parent)
        {
            return System.Windows.LogicalTreeHelper.GetChildren(parent);
        }
    }

    // SessionManager pro udržování informací o relaci
    public static class SessionManager
    {
        private static User _currentUser;
        private static DateTime _loginTime;
        private static bool _rememberMe;

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                if (value != null)
                {
                    _loginTime = DateTime.Now;
                }
            }
        }

        public static DateTime LoginTime => _loginTime;
        public static bool RememberMe { get => _rememberMe; set => _rememberMe = value; }

        public static bool IsLoggedIn => _currentUser != null;

        public static void Logout()
        {
            _currentUser = null;
            _loginTime = default;
            _rememberMe = false;
        }

        public static TimeSpan SessionDuration => DateTime.Now - _loginTime;
    }

    // Event arguments pro uživatelské události
    public class UserEventArgs : EventArgs
    {
        public User User { get; set; }
        public string Action { get; set; }

        public UserEventArgs(User user, string action)
        {
            User = user;
            Action = action;
        }
    }

    // Mock data generator pro testování
    public static class MockDataGenerator
    {
        private static Random _random = new Random();
        private static string[] _firstNames = { "Jan", "Petr", "Pavel", "Tomáš", "Josef", "Jiří", "Jana", "Marie", "Eva", "Anna" };
        private static string[] _lastNames = { "Novák", "Svoboda", "Novotný", "Dvořák", "Černý", "Procházka", "Krejčí", "Horáková", "Němcová", "Pokorná" };
        private static string[] _companies = { "ABC s.r.o.", "XYZ a.s.", "Tech Solutions", "Digital Services", "Modern Business", "Smart Systems" };
        private static string[] _cities = { "Praha", "Brno", "Ostrava", "Plzeň", "Liberec", "Olomouc", "České Budějovice", "Hradec Králové" };

        public static List<Customer> GenerateCustomers(int count = 10)
        {
            var customers = new List<Customer>();
            for (int i = 0; i < count; i++)
            {
                customers.Add(new Customer
                {
                    CustomerId = i + 1,
                    FirstName = _firstNames[_random.Next(_firstNames.Length)],
                    LastName = _lastNames[_random.Next(_lastNames.Length)],
                    Email = $"customer{i + 1}@example.com",
                    Phone = $"+420{_random.Next(100000000, 999999999)}",
                    Company = _random.Next(2) == 0 ? _companies[_random.Next(_companies.Length)] : null,
                    Address = $"Ulice {_random.Next(1, 100)}",
                    City = _cities[_random.Next(_cities.Length)],
                    Country = "Czech Republic",
                    CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 365)),
                    IsActive = true
                });
            }
            return customers;
        }

        public static List<Order> GenerateOrders(int count = 20)
        {
            var orders = new List<Order>();
            var statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            var priorities = new[] { "Low", "Normal", "High", "Urgent" };

            for (int i = 0; i < count; i++)
            {
                var orderDate = DateTime.Now.AddDays(-_random.Next(0, 30));
                orders.Add(new Order
                {
                    OrderId = i + 1,
                    OrderNumber = $"ORD-{DateTime.Now.Year}-{(i + 1):000000}",
                    CustomerId = _random.Next(1, 11), // Assuming 10 customers
                    UserId = 1, // Assuming current user
                    OrderDate = orderDate,
                    Status = statuses[_random.Next(statuses.Length)],
                    Priority = priorities[_random.Next(priorities.Length)],
                    TotalAmount = _random.Next(500, 50000),
                    NetAmount = _random.Next(500, 50000),
                    PaymentStatus = _random.Next(2) == 0 ? "Paid" : "Pending",
                    CreatedAt = orderDate
                });
            }
            return orders;
        }

        public static DashboardStats GenerateStats()
        {
            return new DashboardStats
            {
                TotalOrders = _random.Next(800, 2000),
                PendingOrders = _random.Next(50, 200),
                CompletedToday = _random.Next(10, 50),
                RevenueToday = _random.Next(5000, 25000),
                ActiveCustomers = _random.Next(100, 500),
                ActiveProducts = _random.Next(50, 200)
            };
        }
    }

    public partial class MainWindow : Window
    {
        private Dictionary<string, Button> menuButtons;
        private string currentPage = "Dashboard";
        private User _currentUser;
        private DispatcherTimer _refreshTimer;
        private bool _isLoggingOut = false;

        // Pages cache
        private Dictionary<string, Page> _pageCache = new Dictionary<string, Page>();

        private MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            InitializeWithUser();
        }

        private void InitializeWithUser()
        {
            if (_currentUser == null)
            {
                throw new InvalidOperationException("MainWindow nelze inicializovat bez přihlášeného uživatele.");
            }

            InitializeNavigation();
            UpdateUserInterface();
            InitializeRefreshTimer();

            // Navigace na výchozí stránku
            NavigateToPageInternal("Dashboard");
        }

        private void InitializeNavigation()
        {
            menuButtons = new Dictionary<string, Button>
            {
                {"Dashboard", btnDashboard},
                {"ListOverview", btnListOverview},
                {"OrderStatus", btnOrderStatus},
                {"OrderDetails", btnOrderDetails},
                {"Statistics", btnStatistics},
                {"Reports", btnReports},
                {"Profile", btnProfile}
            };
        }

        private void UpdateUserInterface()
        {
            if (_currentUser != null)
            {
                UserInitials.Text = _currentUser.GetInitials();
                UserFullName.Text = _currentUser.FullName;
                UserRole.Text = _currentUser.Role;
                this.Title = $"Order Management - {_currentUser.Role} Dashboard";
            }
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(5);
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (currentPage == "Dashboard" && !_isLoggingOut)
            {
                // Refresh dashboard if it's the current page
                if (_pageCache.ContainsKey("Dashboard") && _pageCache["Dashboard"] is DashboardPage dashboardPage)
                {
                    dashboardPage.RefreshData();
                }
            }
        }

        private void NavigateToPage(object sender, RoutedEventArgs e)
        {
            if (_isLoggingOut) return;

            if (sender is Button clickedButton && clickedButton.Tag is string pageName)
            {
                NavigateToPageInternal(pageName);
            }
        }

        private void NavigateToPageInternal(string pageName)
        {
            if (_isLoggingOut) return;

            try
            {
                Page pageToShow = GetOrCreatePage(pageName);

                if (pageToShow != null)
                {
                    MainContentFrame.Navigate(pageToShow);
                    currentPage = pageName;
                    UpdatePageTitle(pageName);
                    UpdateMenuButtonStyles(pageName);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při načítání stránky {pageName}: {ex.Message}");
            }
        }

        private Page GetOrCreatePage(string pageName)
        {
            // Zkusit získat stránku z cache
            if (_pageCache.ContainsKey(pageName))
            {
                return _pageCache[pageName];
            }

            // Vytvořit novou stránku
            Page newPage = pageName switch
            {
                "Dashboard" => new DashboardPage(_currentUser),
                //"ListOverview" => new ListOverviewPage(_currentUser),
                //"OrderStatus" => new OrderStatusPage(_currentUser),
                //"OrderDetails" => new OrderDetailsPage(_currentUser),
                //"Statistics" => new StatisticsPage(_currentUser),
                //"Reports" => new ReportsPage(_currentUser),
                //"Profile" => new ProfilePage(_currentUser),
                _ => null
            };

            if (newPage != null)
            {
                _pageCache[pageName] = newPage;

                // Přidat event handlers pro komunikaci mezi pages a main window
                if (newPage is INavigationPage navPage)
                {
                    navPage.NavigationRequested += OnPageNavigationRequested;
                }
            }

            return newPage;
        }

        private void OnPageNavigationRequested(object sender, NavigationEventArgs e)
        {
            // Handler pro navigaci z pages
            NavigateToPageInternal(e.PageName);

            // Případně předat parametry
            if (!string.IsNullOrEmpty(e.Parameter) && _pageCache.ContainsKey(e.PageName))
            {
                if (_pageCache[e.PageName] is IParameterReceiver paramPage)
                {
                    paramPage.ReceiveParameter(e.Parameter);
                }
            }
        }

        private void UpdatePageTitle(string pageName)
        {
            if (_isLoggingOut) return;

            string title = pageName switch
            {
                "Dashboard" => "Dashboard",
                "ListOverview" => "List Overview",
                "OrderStatus" => "Order Status",
                "OrderDetails" => "Order Details",
                "Statistics" => "Statistics",
                "Reports" => "Reports Screen",
                "Profile" => "Profile Settings",
                _ => "Dashboard"
            };

            PageTitle.Text = title;
        }

        private void UpdateMenuButtonStyles(string activePage)
        {
            if (_isLoggingOut) return;

            foreach (var button in menuButtons.Values)
            {
                button.Style = (Style)FindResource("MenuButtonStyle");
            }

            if (menuButtons.ContainsKey(activePage))
            {
                menuButtons[activePage].Style = (Style)FindResource("ActiveMenuButtonStyle");
            }
        }

        public void NavigateTo(string pageName, string parameter = null)
        {
            if (!_isLoggingOut)
            {
                NavigateToPageInternal(pageName);

                if (!string.IsNullOrEmpty(parameter) && _pageCache.ContainsKey(pageName))
                {
                    if (_pageCache[pageName] is IParameterReceiver paramPage)
                    {
                        paramPage.ReceiveParameter(parameter);
                    }
                }
            }
        }

        public string GetCurrentPage()
        {
            return currentPage;
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingOut) return;

            MessageBox.Show("Notification panel is not implemented yet.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingOut) return;

            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "Opravdu se chcete odhlásit?",
                    "Potvrzení odhlášení",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    PerformLogout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při odhlašování: {ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformLogout()
        {
            try
            {
                _isLoggingOut = true;

                // Zastavit timer
                _refreshTimer?.Stop();
                _refreshTimer = null;

                // Cleanup pages
                foreach (var page in _pageCache.Values)
                {
                    if (page is INavigationPage navPage)
                    {
                        navPage.NavigationRequested -= OnPageNavigationRequested;
                    }

                    if (page is IDisposable disposablePage)
                    {
                        disposablePage.Dispose();
                    }
                }
                _pageCache.Clear();

                // Vyčistit uživatelské údaje
                _currentUser = null;

                // Vyčistit session
                SessionManager.Logout();

                // Smazat auto-login soubory
                ClearAutoLoginFiles();

                // Zavřít toto okno a otevřít login
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba při přesměrování na login: {ex.Message}",
                            "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kritická chyba při odhlašování: {ex.Message}",
                    "Kritická chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void ClearAutoLoginFiles()
        {
            try
            {
                const string CREDENTIALS_FILE = "user_credentials.dat";
                const string AUTO_LOGIN_FILE = "auto_login.dat";

                if (System.IO.File.Exists(CREDENTIALS_FILE))
                    System.IO.File.Delete(CREDENTIALS_FILE);

                if (System.IO.File.Exists(AUTO_LOGIN_FILE))
                    System.IO.File.Delete(AUTO_LOGIN_FILE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při mazání auto-login souborů: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            if (!_isLoggingOut)
            {
                MessageBox.Show(message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public bool HasPermission(string action)
        {
            if (_currentUser == null || _isLoggingOut) return false;

            return _currentUser.Role.ToLower() switch
            {
                "admin" => true,
                "manager" => !action.Equals("delete_user", StringComparison.OrdinalIgnoreCase),
                "user" => action.Equals("view", StringComparison.OrdinalIgnoreCase) ||
                         action.Equals("edit_own_profile", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isLoggingOut)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Opravdu chcete ukončit aplikaci?",
                    "Ukončení aplikace",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Cleanup
            _refreshTimer?.Stop();

            foreach (var page in _pageCache.Values)
            {
                if (page is IDisposable disposablePage)
                {
                    disposablePage.Dispose();
                }
            }

            base.OnClosing(e);
        }
    }

    public interface INavigationPage
    {
        event EventHandler<NavigationEventArgs> NavigationRequested;
    }

    public interface IParameterReceiver
    {
        void ReceiveParameter(string parameter);
    }

    public class NavigationEventArgs : EventArgs
    {
        public string PageName { get; set; }
        public string Parameter { get; set; }

        public NavigationEventArgs(string pageName, string parameter = null)
        {
            PageName = pageName;
            Parameter = parameter;
        }
    }
}