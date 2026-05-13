using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class TechCard
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string product_name { get; set; }
        public int version { get; set; }
        public string status { get; set; }
        public string created_at { get; set; }
        public int? step_count { get; set; }
        public List<TechCardStep> steps { get; set; }
    }

    public class TechCardStep
    {
        public int id { get; set; }
        public int card_id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public string step_type { get; set; }
        public decimal? planned_temp_c { get; set; }
        public int? planned_duration_min { get; set; }
        public decimal? planned_pressure_bar { get; set; }
        public decimal? temp_tolerance_max { get; set; }
        public bool is_mandatory { get; set; }
        public string instruction { get; set; }
    }
}
