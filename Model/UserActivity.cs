using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Model
{
    public class UserActivity
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
