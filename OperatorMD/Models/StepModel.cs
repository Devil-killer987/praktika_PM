using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorMD.Models
{
    public class StepDetail
    {
        public int id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public string status { get; set; }
        public decimal? planned_temp_c { get; set; }
        public int? planned_duration_min { get; set; }
        public decimal? planned_pressure_bar { get; set; }
        public decimal? actual_temp_c { get; set; }
        public int? actual_duration_min { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public bool deviation_flag { get; set; }
        public string operator_comment { get; set; }
        public string instruction { get; set; }
        public decimal? temp_tolerance_max { get; set; }
        public decimal? pressure_tolerance_max { get; set; }
    }
}
