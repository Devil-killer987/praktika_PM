using api_work2.Models;
using System.Linq;
using System.Web.Http;

namespace Api_work.Controllers
{
    [RoutePrefix("api/recipes")]
    public class RecipesController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/recipes
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetRecipes()
        {
            var recipes = db.recipes
                .Select(r => new
                {
                    r.id,
                    r.product_id,
                    product_name = r.products.name,  // Плоское поле
                    r.version,
                    r.status,
                    r.sum_percentage,
                    r.created_at,
                    r.approved_at
                })
                .ToList();

            return Ok(recipes);
        }

        // GET: api/recipes/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveRecipes()
        {
            var recipes = db.recipes
                .Where(r => r.status == "active")
                .Select(r => new
                {
                    r.id,
                    r.product_id,
                    product_name = r.products.name,
                    r.version,
                    r.status,
                    r.sum_percentage
                })
                .ToList();

            return Ok(recipes);
        }

        // GET: api/recipes/5/details
        [HttpGet]
        [Route("{id}/details")]
        public IHttpActionResult GetRecipeDetails(int id)
        {
            var recipe = db.recipes
                .Where(r => r.id == id)
                .Select(r => new
                {
                    r.id,
                    r.product_id,
                    product_name = r.products.name,
                    r.version,
                    r.status,
                    r.sum_percentage,
                    r.created_at,
                    r.approved_at,
                    components = r.recipe_components.Select(rc => new
                    {
                        rc.id,
                        rc.material_id,
                        material_name = rc.materials.name,
                        rc.percentage,
                        rc.load_order
                    })
                })
                .FirstOrDefault();

            if (recipe == null)
                return NotFound();

            return Ok(recipe);
        }
    }
}