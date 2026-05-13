using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_work2.DTOs
{
    public class BatchDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DeviationCount { get; set; }
    }

    public class BatchStepDto
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public decimal? ActualTempC { get; set; }
        public int? ActualDurationMin { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public bool DeviationFlag { get; set; }
        public string OperatorComment { get; set; }
    }

    public class StepCompleteDto
    {
        public decimal? ActualTempC { get; set; }
        public int? ActualDurationMin { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public string OperatorComment { get; set; }
        public string Severity { get; set; } // warning, critical
    }
}