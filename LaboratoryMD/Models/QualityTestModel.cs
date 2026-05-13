using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryMD.Models
{
    public class QualityTest
    {
        public int id { get; set; }
        public int? batch_id { get; set; }
        public string batch_number { get; set; }
        public string product_name { get; set; }
        public int? material_id { get; set; }
        public string material_name { get; set; }
        public DateTime analysis_date { get; set; }
        public string sample_type { get; set; } // raw_material, finished_product
        public string status { get; set; } // in_progress, completed
        public string decision { get; set; } // approved, blocked
        public string analyst_comment { get; set; }
        public List<TestResult> results { get; set; }
    }

    public class TestResult
    {
        public int id { get; set; }
        public int test_id { get; set; }
        public string parameter_name { get; set; }
        public string measured_value { get; set; }
        public string standard_value { get; set; }
        public string unit { get; set; }
        public string result { get; set; } // pass, fail
    }

    public class CreateTestRequest
    {
        public int? BatchId { get; set; }
        public int? MaterialId { get; set; }
        public string SampleType { get; set; }
    }

    public class TestResultInput
    {
        public int TestId { get; set; }
        public string Comment { get; set; }
        public List<TestResultItem> Results { get; set; }
    }

    public class TestResultItem
    {
        public string ParameterName { get; set; }
        public string MeasuredValue { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }
        public string Result { get; set; }
    }

    public class DecisionRequest
    {
        public string Decision { get; set; } // approved, blocked
        public string Comment { get; set; }
    }
}
