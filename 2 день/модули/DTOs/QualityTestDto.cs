using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_work2.DTOs
{
    public class QualityTestDto
    {
        public int Id { get; set; }
        public int? BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string SampleType { get; set; }
        public string Status { get; set; }
        public string Decision { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<TestResultDto> Results { get; set; }
    }

    public class TestResultDto
    {
        public string ParameterName { get; set; }
        public string MeasuredValue { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }
        public string Result { get; set; } // pass/fail
    }

    public class TestResultInputDto
    {
        public int TestId { get; set; }
        public List<TestResultDto> Results { get; set; }
        public string Comment { get; set; }
    }

    public class DecisionDto
    {
        public int TestId { get; set; }
        public string Decision { get; set; } // approved / blocked
        public string Comment { get; set; }
    }
}