using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class Batch
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public int order_id { get; set; }
        public string order_number { get; set; }
        public string product_name { get; set; }
        public string status { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public decimal? actual_quantity_kg { get; set; }
        public int deviation_count { get; set; }
        public string created_at { get; set; }
    }

    public class BatchStep
    {
        public int id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public decimal? actual_temp_c { get; set; }
        public int? actual_duration_min { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public bool deviation_flag { get; set; }
        public string operator_comment { get; set; }
        public decimal? planned_temp_c { get; set; }
        public int? planned_duration_min { get; set; }
        public decimal? planned_pressure_bar { get; set; }
    }
}
