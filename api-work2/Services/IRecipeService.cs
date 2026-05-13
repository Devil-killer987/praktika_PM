using api_work2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace api_work2.Services
{
    public interface IRecipeService
    {
        Task<List<recipes>> GetRecipes(string status = null);
        Task<recipes> GetRecipeById(int id);
        Task<recipes> CreateRecipe(recipes recipe);
        Task<recipes> ActivateRecipe(int id);
    }
}