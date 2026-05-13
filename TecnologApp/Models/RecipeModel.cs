using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnologApp.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string product_name { get; set; }
        public int version { get; set; }
        public string status { get; set; }
        public decimal sum_percentage { get; set; }
        public string created_at { get; set; }
        public string created_by { get; set; }
        public string approved_at { get; set; }
        public string approved_by { get; set; }
        public List<RecipeComponent> components { get; set; }
    }

    public class RecipeComponent
    {
        public int id { get; set; }
        public int recipe_id { get; set; }
        public int material_id { get; set; }
        public string material_name { get; set; }
        public decimal percentage { get; set; }
        public int load_order { get; set; }
        public decimal tolerance_min { get; set; }
        public decimal tolerance_max { get; set; }
    }

    public class CreateRecipeRequest
    {
        public int product_id { get; set; }
        public string description { get; set; }
    }

    public class ActivateRecipeRequest
    {
        public int ApprovedBy { get; set; }
    }
}
