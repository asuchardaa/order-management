using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Model
{
    public class DashboardStats
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int ActiveCustomers { get; set; }
        public int ActiveProducts { get; set; }
    }
}
