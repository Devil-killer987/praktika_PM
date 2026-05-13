using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryMD.Models
{
    public class Batch
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public string order_number { get; set; }
        public string product_name { get; set; }
        public string status { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public decimal? actual_quantity_kg { get; set; }
        public int deviation_count { get; set; }
    }

    public class RawMaterial
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string material_type { get; set; }
        public string supplier { get; set; }
        public bool is_active { get; set; }
    }
}
