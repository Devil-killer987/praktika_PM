using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class ProductionOrder
    {
        public int id { get; set; }
        public string order_number { get; set; }
        public int recipe_id { get; set; }
        public string product_name { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public string status { get; set; }
        public DateTime? planned_start_date { get; set; }
    }

    public class CreateOrderRequest
    {
        public int recipe_id { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public DateTime planned_start_date { get; set; }
    }
}
