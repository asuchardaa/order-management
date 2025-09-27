using OrderManagement.Model;
using OrderManagement.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private Dictionary<string, Grid> contentPages;
        private Dictionary<string, Button> menuButtons;
        private string currentPage = "Dashboard";
        private User _currentUser;
        private DispatcherTimer _refreshTimer;

        // Constructor for when user is not logged in (redirect to login)
        public MainWindow()
        {
            InitializeComponent();

            // If no user provided, redirect to login
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Constructor with authenticated user
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            InitializeWithUser();
        }

        private void InitializeWithUser()
        {
            InitializeNavigation();
            UpdateUserInterface();
            InitializeRefreshTimer();
            LoadDashboardData();
        }

        private void InitializeNavigation()
        {
            // Initialize content pages dictionary
            contentPages = new Dictionary<string, Grid>
            {
                {"Dashboard", DashboardContent},
                {"ListOverview", ListOverviewContent},
                {"OrderStatus", OrderStatusContent},
                {"OrderDetails", OrderDetailsContent},
                {"Statistics", StatisticsContent},
                {"Reports", ReportsContent},
                {"Profile", ProfileContent}
            };

            // Initialize menu buttons dictionary
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

            // Set initial page
            ShowPage("Dashboard");
        }

        private void UpdateUserInterface()
        {
            if (_currentUser != null)
            {
                // Update welcome message
                var welcomeText = this.FindName("WelcomeText") as TextBlock;
                if (welcomeText != null)
                {
                    welcomeText.Text = $"Welcome back, {_currentUser.FullName.Split(' ')[0]}!";
                }

                // Update user info in sidebar
                UpdateUserInfoDisplay();

                // Update window title
                this.Title = $"OrderMaster - {_currentUser.Role} Dashboard";
            }
        }

        private void UpdateUserInfoDisplay()
        {
            // Find user info elements in the sidebar and update them
            // Update the TextBlocks that are already in the XAML

            // Find the user info section in the sidebar
            var userInfoSection = this.FindName("NavigationMenu") as StackPanel;
            if (userInfoSection != null)
            {
                // The XAML already has hardcoded user info, we'll update it dynamically
                // Find all TextBlocks in the user info area and update them
                UpdateUserInfoInSidebar();
            }
        }

        private void UpdateUserInfoInSidebar()
        {
            // Since the XAML has hardcoded values, we'll update them programmatically
            // This is a bit hacky but works with the existing XAML structure

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Find all TextBlocks and update the user-related ones
                foreach (var element in LogicalTreeHelper.GetChildren(this))
                {
                    if (element is Grid grid)
                    {
                        UpdateTextBlocksRecursively(grid);
                    }
                }
            }));
        }

        private void UpdateTextBlocksRecursively(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBlock textBlock)
                {
                    // Update specific TextBlocks based on their content
                    if (textBlock.Text == "JD")
                    {
                        textBlock.Text = _currentUser.GetInitials();
                    }
                    else if (textBlock.Text == "John Doe")
                    {
                        textBlock.Text = _currentUser.FullName;
                    }
                    else if (textBlock.Text == "Administrator")
                    {
                        textBlock.Text = _currentUser.Role;
                    }
                    else if (textBlock.Text == "Welcome back, John!")
                    {
                        textBlock.Text = $"Welcome back, {_currentUser.FullName.Split(' ')[0]}!";
                    }
                }

                UpdateTextBlocksRecursively(child);
            }
        }

        private void InitializeRefreshTimer()
        {
            // Initialize a timer to refresh dashboard data periodically
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(5); // Refresh every 5 minutes
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (currentPage == "Dashboard")
            {
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            // Simulate loading dashboard data
            // In a real application, this would fetch data from a service/database

            // Update statistics with mock data
            UpdateDashboardStats();
        }

        private void UpdateDashboardStats()
        {
            // Generate some mock statistics
            var random = new Random();
            var totalOrders = random.Next(1000, 2000);
            var pendingOrders = random.Next(50, 150);
            var completedToday = random.Next(20, 80);
            var revenue = random.Next(10000, 50000);

            // Update the statistics in the UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateStatisticCards(totalOrders, pendingOrders, completedToday, revenue);
            }));
        }

        private void UpdateStatisticCards(int totalOrders, int pendingOrders, int completedToday, decimal revenue)
        {
            // Find and update statistic cards
            // This method updates the hardcoded values in the XAML

            foreach (var element in LogicalTreeHelper.GetChildren(this))
            {
                if (element is Grid grid)
                {
                    UpdateStatCardsRecursively(grid, totalOrders, pendingOrders, completedToday, revenue);
                }
            }
        }

        private void UpdateStatCardsRecursively(DependencyObject parent, int totalOrders, int pendingOrders, int completedToday, decimal revenue)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBlock textBlock)
                {
                    // Update specific statistic values
                    if (textBlock.Text == "1,234")
                    {
                        textBlock.Text = totalOrders.ToString("N0");
                    }
                    else if (textBlock.Text == "87")
                    {
                        textBlock.Text = pendingOrders.ToString("N0");
                    }
                    else if (textBlock.Text == "45")
                    {
                        textBlock.Text = completedToday.ToString("N0");
                    }
                    else if (textBlock.Text == "$12,450")
                    {
                        textBlock.Text = $"${revenue:N0}";
                    }
                }

                UpdateStatCardsRecursively(child, totalOrders, pendingOrders, completedToday, revenue);
            }
        }

        private void NavigateToPage(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Extract page name from button name (remove "btn" prefix)
                string pageName = clickedButton.Name.Substring(3);

                // Handle special case for List Overview
                if (pageName == "ListOverview")
                {
                    ShowPage("ListOverview");
                }
                else
                {
                    ShowPage(pageName);
                }
            }
        }

        private void ShowPage(string pageName)
        {
            // Hide all pages
            foreach (var page in contentPages.Values)
            {
                page.Visibility = Visibility.Collapsed;
            }

            // Show selected page
            if (contentPages.ContainsKey(pageName))
            {
                contentPages[pageName].Visibility = Visibility.Visible;
                currentPage = pageName;

                // Update page title
                UpdatePageTitle(pageName);

                // Update menu button styles
                UpdateMenuButtonStyles(pageName);

                // Load page-specific data
                LoadPageData(pageName);
            }
        }

        private void LoadPageData(string pageName)
        {
            switch (pageName)
            {
                case "Dashboard":
                    LoadDashboardData();
                    break;
                case "Profile":
                    LoadProfileData();
                    break;
                    // Add other page data loading as needed
            }
        }

        private void LoadProfileData()
        {
            // This would load and display user profile information
            // For now, we'll just ensure the profile page shows current user info
        }

        private void UpdatePageTitle(string pageName)
        {
            string title;
            switch (pageName)
            {
                case "Dashboard":
                    title = "Dashboard";
                    break;
                case "ListOverview":
                    title = "List Overview";
                    break;
                case "OrderStatus":
                    title = "Order Status";
                    break;
                case "OrderDetails":
                    title = "Order Details";
                    break;
                case "Statistics":
                    title = "Statistics";
                    break;
                case "Reports":
                    title = "Reports Screen";
                    break;
                case "Profile":
                    title = "Profile Settings";
                    break;
                default:
                    title = "Dashboard";
                    break;
            }

            PageTitle.Text = title;
        }

        private void UpdateMenuButtonStyles(string activePage)
        {
            // Reset all buttons to normal style
            foreach (var button in menuButtons.Values)
            {
                button.Style = (Style)FindResource("MenuButtonStyle");
            }

            // Set active button style
            if (menuButtons.ContainsKey(activePage))
            {
                menuButtons[activePage].Style = (Style)FindResource("ActiveMenuButtonStyle");
            }
        }

        // Method to programmatically navigate (can be called from other parts of the application)
        public void NavigateTo(string pageName)
        {
            ShowPage(pageName);
        }

        // Method to get current page
        public string GetCurrentPage()
        {
            return currentPage;
        }

        // Event handlers for specific functionality
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Implementation for search functionality
            // This would typically filter content based on search terms
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for notification panel
            // This would show a dropdown or popup with notifications
            MessageBox.Show("Notification panel is not implemented yet.",
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for logout functionality
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Stop the refresh timer
                _refreshTimer?.Stop();

                // Clear current user
                _currentUser = null;

                // Close current window and show login window
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        // Method to update statistics (can be called periodically)
        public void RefreshDashboardStats()
        {
            UpdateDashboardStats();
        }

        // Helper method to show notifications/messages to user
        public void ShowNotification(string message, string type = "info")
        {
            MessageBox.Show(message, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Method to handle window closing
        //protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        //{
        //    MessageBoxResult result = MessageBox.Show(
        //        "Are you sure you want to exit OrderMaster?",
        //        "Exit Application",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Question);

        //    if (result == MessageBoxResult.No)
        //    {
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        // Stop the refresh timer
        //        _refreshTimer?.Stop();
        //    }

        //    base.OnClosing(e);
        //}

        // Method to get current user
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        // Method to check if user has permission for certain actions
        public bool HasPermission(string action)
        {
            if (_currentUser == null) return false;

            // Simple role-based permissions
            switch (_currentUser.Role.ToLower())
            {
                case "admin":
                    return true; // Admin can do everything
                case "manager":
                    return !action.Equals("delete_user", StringComparison.OrdinalIgnoreCase);
                case "user":
                    return action.Equals("view", StringComparison.OrdinalIgnoreCase) ||
                           action.Equals("edit_own_profile", StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }
    }

    // Extension class for additional navigation helpers
    public static class NavigationExtensions
    {
        public static void NavigateToOrderDetails(this MainWindow mainWindow, int orderId)
        {
            mainWindow.NavigateTo("OrderDetails");
            // Additional logic to load specific order details
        }

        public static void NavigateToOrderStatus(this MainWindow mainWindow, string status = null)
        {
            mainWindow.NavigateTo("OrderStatus");
            // Additional logic to filter by status if provided
        }
    }
}