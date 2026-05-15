using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorMD.Models
{
    public class ActiveBatch
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public string product_name { get; set; }
        public string line { get; set; }
        public string status { get; set; }
        public string current_step { get; set; }
        public int current_step_progress { get; set; }
        public int total_steps { get; set; }
        public bool has_deviation { get; set; }
        public DateTime? start_time { get; set; }
    }

    public class BatchProgram
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public string product_name { get; set; }
        public string status { get; set; }
        public DateTime? start_time { get; set; }
        public List<BatchStep> steps { get; set; }
        public int current_step_id { get; set; }
    }

    public class BatchStep
    {
        public int id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public string status { get; set; } // pending, in_progress, completed
        public decimal? planned_temp_c { get; set; }
        public decimal? planned_pressure_bar { get; set; }
        public int? planned_duration_min { get; set; }
        public decimal? actual_temp_c { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public int? actual_duration_min { get; set; }
        public bool deviation_flag { get; set; }
        public string operator_comment { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string instruction { get; set; }
        public bool is_mandatory { get; set; }
        public decimal? temp_tolerance_max { get; set; }
        public decimal? pressure_tolerance_max { get; set; }
    }

    public class StartStepResponse
    {
        public string message { get; set; }
        public int stepId { get; set; }
    }

    public class CompleteStepRequest
    {
        public decimal? ActualTempC { get; set; }
        public int? ActualDurationMin { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public string OperatorComment { get; set; }
        public string Severity { get; set; }
    }

    public class CompleteStepResponse
    {
        public int step_id { get; set; }
        public string step_name { get; set; }
        public bool completed { get; set; }
        public bool deviation_flag { get; set; }
        public bool all_steps_completed { get; set; }
    }
}
