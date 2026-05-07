using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_work2.DTOs
{
    public class RecipeDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public decimal SumPercentage { get; set; }
        public List<RecipeComponentDto> Components { get; set; }
    }

    public class RecipeComponentDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal Percentage { get; set; }
        public int LoadOrder { get; set; }
    }
}