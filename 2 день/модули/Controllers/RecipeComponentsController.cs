
using api_work2.Models;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/recipecomponents")]
    public class RecipeComponentsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/recipecomponents
        public IQueryable<recipe_components> Getrecipe_components()
        {
            return db.recipe_components;
        }

        // GET: api/recipecomponents/5
        [ResponseType(typeof(recipe_components))]
        public IHttpActionResult Getrecipe_components(int id)
        {
            recipe_components recipe_components = db.recipe_components.Find(id);
            if (recipe_components == null)
            {
                return NotFound();
            }

            return Ok(recipe_components);
        }

        // GET: api/recipecomponents/byrecipe/5
        [HttpGet]
        [Route("byrecipe/{recipeId}")]
        public IHttpActionResult GetComponentsByRecipe(int recipeId)
        {
            var components = db.recipe_components
                .Where(rc => rc.recipe_id == recipeId)
                .OrderBy(rc => rc.load_order)
                .Select(rc => new
                {
                    rc.id,
                    rc.material_id,
                    material_name = rc.materials.name,
                    material_code = rc.materials.code,
                    rc.percentage,
                    rc.load_order,
                    rc.tolerance_min,
                    rc.tolerance_max
                })
                .ToList();

            return Ok(components);
        }

        // POST: api/recipecomponents
        [ResponseType(typeof(recipe_components))]
        public IHttpActionResult Postrecipe_components(recipe_components recipe_components)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipe = db.recipes.Find(recipe_components.recipe_id);
            if (recipe == null)
            {
                return BadRequest("Recipe not found");
            }

            if (recipe.status == "active")
            {
                return BadRequest("Cannot modify components of active recipe");
            }

            // Check load order uniqueness
            if (db.recipe_components.Any(rc => rc.recipe_id == recipe_components.recipe_id && rc.load_order == recipe_components.load_order))
            {
                return BadRequest($"Load order {recipe_components.load_order} already exists for this recipe");
            }

            db.recipe_components.Add(recipe_components);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = recipe_components.id }, recipe_components);
        }

        // DELETE: api/recipecomponents/5
        [ResponseType(typeof(recipe_components))]
        public IHttpActionResult Deleterecipe_components(int id)
        {
            recipe_components recipe_components = db.recipe_components.Find(id);
            if (recipe_components == null)
            {
                return NotFound();
            }

            var recipe = db.recipes.Find(recipe_components.recipe_id);
            if (recipe.status == "active")
            {
                return BadRequest("Cannot delete components from active recipe");
            }

            db.recipe_components.Remove(recipe_components);
            db.SaveChanges();

            return Ok(recipe_components);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}