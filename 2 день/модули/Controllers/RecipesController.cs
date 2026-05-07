
using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/recipes")]
    public class RecipesController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/recipes
        public IQueryable<recipes> Getrecipes()
        {
            return db.recipes;
        }

        // GET: api/recipes/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveRecipes()
        {
            var recipes = db.recipes.Where(r => r.status == "active")
                .Select(r => new
                {
                    r.id,
                    product_name = r.products.name,
                    r.version,
                    r.sum_percentage,
                    r.created_at,
                    components = r.recipe_components.Select(rc => new
                    {
                        rc.material_id,
                        material_name = rc.materials.name,
                        rc.percentage,
                        rc.load_order
                    })
                })
                .ToList();

            return Ok(recipes);
        }

        // GET: api/recipes/5
        [ResponseType(typeof(recipes))]
        public IHttpActionResult Getrecipes(int id)
        {
            recipes recipes = db.recipes.Find(id);
            if (recipes == null)
            {
                return NotFound();
            }

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
                    product_id = r.product_id,
                    product_name = r.products.name,
                    r.version,
                    r.status,
                    r.sum_percentage,
                    r.created_by,
                    r.created_at,
                    r.approved_by,
                    r.approved_at,
                    r.description,
                    components = r.recipe_components
                        .OrderBy(rc => rc.load_order)
                        .Select(rc => new
                        {
                            rc.id,
                            rc.material_id,
                            material_name = rc.materials.name,
                            rc.percentage,
                            rc.load_order,
                            rc.tolerance_min,
                            rc.tolerance_max
                        })
                })
                .FirstOrDefault();

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        // POST: api/recipes
        [ResponseType(typeof(recipes))]
        public IHttpActionResult Postrecipes(recipes recipes)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if product exists
            if (!db.products.Any(p => p.id == recipes.product_id))
            {
                return BadRequest("Product not found");
            }

            // Get next version number
            var maxVersion = db.recipes
                .Where(r => r.product_id == recipes.product_id)
                .Select(r => (int?)r.version)
                .Max() ?? 0;
            recipes.version = maxVersion + 1;
            recipes.status = "draft";
            recipes.created_at = DateTime.Now;

            db.recipes.Add(recipes);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = recipes.id }, recipes);
        }

        // POST: api/recipes/5/activate
        [HttpPost]
        [Route("{id}/activate")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ActivateRecipe(int id, [FromBody] ActivateRequest request)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null)
            {
                return NotFound();
            }

            if (recipe.status == "active")
            {
                return BadRequest("Recipe is already active");
            }

            // Check sum of percentages
            var sum = db.recipe_components
                .Where(rc => rc.recipe_id == id)
                .Select(rc => (decimal?)rc.percentage)
                .Sum() ?? 0;

            if (sum != 100)
            {
                return BadRequest($"Cannot activate recipe. Sum of components is {sum}%, must be 100%");
            }

            // Deactivate other versions
            var activeRecipes = db.recipes.Where(r => r.product_id == recipe.product_id && r.status == "active");
            foreach (var activeRecipe in activeRecipes)
            {
                activeRecipe.status = "archived";
            }

            recipe.status = "active";
            recipe.approved_by = request.ApprovedBy;
            recipe.approved_at = DateTime.Now;
            recipe.sum_percentage = sum;
            db.SaveChanges();

            return Ok($"Recipe version {recipe.version} activated");
        }

        // PUT: api/recipes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putrecipes(int id, recipes recipes)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != recipes.id)
            {
                return BadRequest();
            }

            var existing = db.recipes.Find(id);
            if (existing.status == "active")
            {
                return BadRequest("Cannot edit active recipe. Create new version instead.");
            }

            db.Entry(recipes).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/recipes/5
        [ResponseType(typeof(recipes))]
        public IHttpActionResult Deleterecipes(int id)
        {
            recipes recipes = db.recipes.Find(id);
            if (recipes == null)
            {
                return NotFound();
            }

            if (recipes.status == "active")
            {
                return BadRequest("Cannot delete active recipe. Archive it instead.");
            }

            db.recipes.Remove(recipes);
            db.SaveChanges();

            return Ok(recipes);
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

    public class ActivateRequest
    {
        public int ApprovedBy { get; set; }
    }
}