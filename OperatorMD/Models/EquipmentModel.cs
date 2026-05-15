using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorMD.Models
{
    public class TelemetryData
    {
        public decimal current_temperature { get; set; }
        public decimal current_pressure { get; set; }
        public int current_rpm { get; set; }
        public string equipment_status { get; set; }
        public string last_update { get; set; }
    }
}
