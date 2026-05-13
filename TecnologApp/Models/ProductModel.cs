using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class Product
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string product_type { get; set; }
        public string release_form { get; set; }
        public string status { get; set; }
        public string created_at { get; set; }
    }
}
