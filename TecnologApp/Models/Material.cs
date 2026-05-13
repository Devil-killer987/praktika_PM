using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class Material
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string material_type { get; set; }
        public string unit { get; set; }
        public string supplier { get; set; }
        public bool is_active { get; set; }
    }
}
