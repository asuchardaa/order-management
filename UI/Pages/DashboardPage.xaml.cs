using OrderManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OrderManagement.Pages
{
    public partial class DashboardPage : Page, INavigationPage
    {
        private User _currentUser;
        private DispatcherTimer _refreshTimer;
        private DashboardStats _currentStats;

        public event EventHandler<NavigationEventArgs> NavigationRequested;

        public DashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += DashboardPage_Loaded;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeMessage();
            await RefreshDataAsync();
            InitializeRefreshTimer();
        }

        private void UpdateWelcomeMessage()
        {
            if (_currentUser != null)
            {
                var firstName = _currentUser.FullName.Split(' ')[0];
                txtWelcome.Text = $"Welcome back, {firstName}!";
            }
        }

        public async void RefreshData()
        {
            await RefreshDataAsync();
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                // Simulace načítání dat
                await Task.Delay(300);

                // Generování mock statistik
                _currentStats = MockDataGenerator.GenerateStats();

                UpdateStatisticsCards();
                await LoadRecentOrdersAsync();
                UpdateChart();
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při načítání dat: {ex.Message}");
            }
        }

        private void UpdateStatisticsCards()
        {
            if (_currentStats == null) return;

            // Total Orders
            txtTotalOrders.Text = _currentStats.TotalOrders.ToString("N0");
            var totalChange = CalculateChange(_currentStats.TotalOrders, _currentStats.TotalOrders - 50);
            txtTotalOrdersChange.Text = $"{totalChange:+0;-0;0}% from last month";
            txtTotalOrdersChange.Foreground = totalChange >= 0 ?
                (Brush)new BrushConverter().ConvertFrom("#10b981") :
                (Brush)new BrushConverter().ConvertFrom("#ef4444");

            // Pending Orders
            txtPendingOrders.Text = _currentStats.PendingOrders.ToString("N0");
            var pendingChange = CalculateChange(_currentStats.PendingOrders, _currentStats.PendingOrders + 5);
            txtPendingOrdersChange.Text = $"{pendingChange:+0;-0;0}% from yesterday";
            txtPendingOrdersChange.Foreground = pendingChange <= 0 ?
                (Brush)new BrushConverter().ConvertFrom("#10b981") :
                (Brush)new BrushConverter().ConvertFrom("#ef4444");

            // Completed Today
            txtCompletedToday.Text = _currentStats.CompletedToday.ToString("N0");
            var completedChange = CalculateChange(_currentStats.CompletedToday, _currentStats.CompletedToday - 3);
            txtCompletedTodayChange.Text = $"{completedChange:+0;-0;0}% from yesterday";
            txtCompletedTodayChange.Foreground = completedChange >= 0 ?
                (Brush)new BrushConverter().ConvertFrom("#10b981") :
                (Brush)new BrushConverter().ConvertFrom("#ef4444");

            // Revenue Today
            txtRevenueToday.Text = $"${_currentStats.RevenueToday:N0}";
            var revenueChange = CalculateChange(_currentStats.RevenueToday, _currentStats.RevenueToday - 1000);
            txtRevenueTodayChange.Text = $"{revenueChange:+0;-0;0}% from yesterday";
            txtRevenueTodayChange.Foreground = revenueChange >= 0 ?
                (Brush)new BrushConverter().ConvertFrom("#10b981") :
                (Brush)new BrushConverter().ConvertFrom("#ef4444");
        }

        private double CalculateChange(decimal current, decimal previous)
        {
            if (previous == 0) return 0;
            return (double)((current - previous) / previous * 100);
        }

        private async Task LoadRecentOrdersAsync()
        {
            try
            {
                // Generování recent orders
                var recentOrders = MockDataGenerator.GenerateOrders(5);
                var customers = MockDataGenerator.GenerateCustomers(10);

                recentOrdersList.Children.Clear();

                foreach (var order in recentOrders)
                {
                    var customer = customers.FirstOrDefault(c => c.CustomerId == order.CustomerId) ?? customers.First();

                    var orderCard = CreateOrderCard(order, customer);
                    recentOrdersList.Children.Add(orderCard);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při načítání posledních objednávek: {ex.Message}");
            }
        }

        private Border CreateOrderCard(Order order, Customer customer)
        {
            var card = new Border
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#333"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stackPanel = new StackPanel();

            // Header with order number and amount
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var orderNumberBlock = new TextBlock
            {
                Text = order.OrderNumber,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                FontSize = 12
            };
            Grid.SetColumn(orderNumberBlock, 0);

            var amountBlock = new TextBlock
            {
                Text = $"${order.NetAmount:N0}",
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#10b981"),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(amountBlock, 1);

            headerGrid.Children.Add(orderNumberBlock);
            headerGrid.Children.Add(amountBlock);

            // Customer name
            var customerBlock = new TextBlock
            {
                Text = customer.FullName,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#b0b0b0"),
                FontSize = 10,
                Margin = new Thickness(0, 2, 0, 0)
            };

            // Status badge
            var statusBorder = new Border
            {
                Background = GetStatusColor(order.Status),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(6, 2, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var statusBlock = new TextBlock
            {
                Text = order.Status,
                Foreground = Brushes.White,
                FontSize = 9
            };

            statusBorder.Child = statusBlock;

            stackPanel.Children.Add(headerGrid);
            stackPanel.Children.Add(customerBlock);
            stackPanel.Children.Add(statusBorder);

            card.Child = stackPanel;

            // Click event
            card.MouseLeftButtonUp += (s, e) => {
                NavigationRequested?.Invoke(this, new NavigationEventArgs("OrderDetails", order.OrderId.ToString()));
            };

            return card;
        }

        private Brush GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => (Brush)new BrushConverter().ConvertFrom("#f59e0b"),
                "Processing" => (Brush)new BrushConverter().ConvertFrom("#4a9eff"),
                "Shipped" => (Brush)new BrushConverter().ConvertFrom("#8b5cf6"),
                "Delivered" => (Brush)new BrushConverter().ConvertFrom("#10b981"),
                "Cancelled" => (Brush)new BrushConverter().ConvertFrom("#ef4444"),
                _ => (Brush)new BrushConverter().ConvertFrom("#6b7280")
            };
        }

        private void UpdateChart()
        {
            chartCanvas.Children.Clear();

            // Vytvoření jednoduchého line chart
            var points = new List<Point>();
            var random = new Random();

            for (int i = 0; i < 7; i++)
            {
                var x = i * 40 + 20;
                var y = 250 - random.Next(50, 200);
                points.Add(new Point(x, y));
            }

            // Vykreslení čar
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = (Brush)new BrushConverter().ConvertFrom("#4a9eff"),
                    StrokeThickness = 2
                };

                chartCanvas.Children.Add(line);
            }

            // Vykreslení bodů
            foreach (var point in points)
            {
                var ellipse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = (Brush)new BrushConverter().ConvertFrom("#4a9eff")
                };

                Canvas.SetLeft(ellipse, point.X - 3);
                Canvas.SetTop(ellipse, point.Y - 3);

                chartCanvas.Children.Add(ellipse);
            }

            // Přidání popisků
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int i = 0; i < days.Length; i++)
            {
                var label = new TextBlock
                {
                    Text = days[i],
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#666"),
                    FontSize = 10
                };

                Canvas.SetLeft(label, i * 40 + 10);
                Canvas.SetTop(label, 270);

                chartCanvas.Children.Add(label);
            }
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(5);
            _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
            _refreshTimer.Start();
        }

        private async void RefreshChart_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }

        private void ViewAllOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationRequested?.Invoke(this, new NavigationEventArgs("ListOverview"));
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }
    }
}