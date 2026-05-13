using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class DashboardStats
    {
        public int active_products { get; set; }
        public int active_recipes { get; set; }
        public int active_tech_cards { get; set; }
        public int orders_in_progress { get; set; }
        public int batches_in_production { get; set; }
        public int batches_with_deviations { get; set; }
        public int pending_quality_tests { get; set; }
        public int blocked_batches { get; set; }
    }

    public class RecentEvent
    {
        public string type { get; set; }
        public string batch_number { get; set; }
        public string product_name { get; set; }
        public string event_time { get; set; }
        public string message { get; set; }
    }
}
